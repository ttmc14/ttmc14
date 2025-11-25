using Content.Shared._MC.Xeno.Sunder;
using Content.Shared._RMC14.Weapons.Ranged;
using Content.Shared.Damage;
using Content.Shared.Examine;
using Content.Shared.Explosion;
using Content.Shared.Inventory;
using Content.Shared.Tag;
using Content.Shared.Verbs;
using Content.Shared.Weapons.Melee;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Shared._MC.Armor;

public sealed class MCArmorSystem : EntitySystem
{
    private static readonly ProtoId<TagPrototype> TagMelee = "MCDamageMelee";
    private static readonly ProtoId<TagPrototype> TagBullet = "MCDamageBullet";
    private static readonly ProtoId<TagPrototype> TagLaser = "MCDamageLaser";
    private static readonly ProtoId<TagPrototype> TagEnergy = "MCDamageEnergy";
    private static readonly ProtoId<TagPrototype> TagBomb = "MCDamageBomb";
    private static readonly ProtoId<TagPrototype> TagBio = "MCDamageBio";
    private static readonly ProtoId<TagPrototype> TagFire = "MCDamageFire";
    private static readonly ProtoId<TagPrototype> TagAcid = "MCDamageAcid";

    [Dependency] private readonly TagSystem _tag = null!;
    [Dependency] private readonly InventorySystem _inventory = null!;
    [Dependency] private readonly ExamineSystemShared _examine = null!;

    [Dependency] private readonly MCXenoSunderSystem _mcXenoSunder = null!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MCArmorComponent, MCArmorGetEvent>(OnGet);
        SubscribeLocalEvent<MCArmorComponent, InventoryRelayedEvent<MCArmorGetEvent>>(OnInventoryGetRelayed);


        SubscribeLocalEvent<MCArmorComponent, GetExplosionResistanceEvent>(OnGetExplosionResistance);
        SubscribeLocalEvent<MCArmorComponent, InventoryRelayedEvent<GetExplosionResistanceEvent>>(OnGetExplosionResistanceRelayed);

        SubscribeLocalEvent<MCArmorComponent, DamageModifyEvent>(OnDamageModify);
        SubscribeLocalEvent<MCArmorComponent, GetVerbsEvent<ExamineVerb>>(OnArmorVerbExamine);

        SubscribeLocalEvent<InventoryComponent, MCArmorGetEvent>(_inventory.RelayEvent);
    }

    private static void OnGet(Entity<MCArmorComponent> entity, ref MCArmorGetEvent args)
    {
        args.ArmorDefinition += entity.Comp.Soft;
    }

    private static void OnInventoryGetRelayed(Entity<MCArmorComponent> entity, ref InventoryRelayedEvent<MCArmorGetEvent> args)
    {
        args.Args.ArmorDefinition += entity.Comp.Soft;
    }

    private void OnGetExplosionResistance(Entity<MCArmorComponent> entity, ref GetExplosionResistanceEvent args)
    {
        args.DamageCoefficient *= ArmorToValue(entity.Comp.Soft.Bomb, 0, _mcXenoSunder.GetSunder(entity.Owner));
    }

    private void OnGetExplosionResistanceRelayed(Entity<MCArmorComponent> entity, ref InventoryRelayedEvent<GetExplosionResistanceEvent> args)
    {
        args.Args.DamageCoefficient *= ArmorToValue(entity.Comp.Soft.Bomb, 0, _mcXenoSunder.GetSunder(entity.Owner));
    }

    private void OnDamageModify(Entity<MCArmorComponent> entity, ref DamageModifyEvent args)
    {
        DamageModify(entity, ref args);
    }

    private void DamageModify(EntityUid entityUid, ref DamageModifyEvent args)
    {
        var ev = new MCArmorGetEvent(SlotFlags.OUTERCLOTHING | SlotFlags.INNERCLOTHING, new MCArmorDefinition());
        RaiseLocalEvent(entityUid, ref ev);

        var sunderModifier = _mcXenoSunder.GetSunder(entityUid);

        if (args.Tool is not { } tool)
            return;

        if (_tag.HasTag(tool, TagMelee) || HasComp<MeleeWeaponComponent>(tool))
        {
            args.Damage *= ArmorToValue(ev.ArmorDefinition.Melee, args.ArmorPiercing, sunderModifier);
            return;
        }

        if (_tag.HasTag(tool, TagBullet) || HasComp<RMCBulletComponent>(tool))
        {
            args.Damage *= ArmorToValue(ev.ArmorDefinition.Bullet, args.ArmorPiercing, sunderModifier);
            return;
        }

        if (_tag.HasTag(tool, TagAcid))
        {
            args.Damage *= ArmorToValue(ev.ArmorDefinition.Acid, args.ArmorPiercing, sunderModifier);
            return;
        }

        if (_tag.HasTag(tool, TagFire))
        {
            args.Damage *= ArmorToValue(ev.ArmorDefinition.Fire, args.ArmorPiercing, sunderModifier);
            return;
        }

        if (_tag.HasTag(tool, TagLaser))
        {
            args.Damage *= ArmorToValue(ev.ArmorDefinition.Laser, args.ArmorPiercing, sunderModifier);
            return;
        }

        if (_tag.HasTag(tool, TagEnergy))
        {
            args.Damage *= ArmorToValue(ev.ArmorDefinition.Energy, args.ArmorPiercing, sunderModifier);
            return;
        }

        if (_tag.HasTag(tool, TagBomb))
        {
            args.Damage *= ArmorToValue(ev.ArmorDefinition.Bomb, args.ArmorPiercing, sunderModifier);
            return;
        }

        if (_tag.HasTag(tool, TagBio))
        {
            args.Damage *= ArmorToValue(ev.ArmorDefinition.Bio, args.ArmorPiercing, sunderModifier);
            return;
        }
    }

    private static float ArmorToValue(int armor, int penetration, float sunder)
    {
        return Math.Clamp((100 - armor * sunder + penetration) * 0.01f, 0, 1);
    }

    private void OnArmorVerbExamine(Entity<MCArmorComponent> entity, ref GetVerbsEvent<ExamineVerb> args)
    {
        if (!args.CanInteract || !args.CanAccess || !entity.Comp.ShowExamine)
            return;

        var examineMarkup = GetArmorExamine(entity);
        _examine.AddDetailedExamineVerb(
            args,
            entity,
            examineMarkup,
            Loc.GetString("mc-armor-examinable-verb-text"),
            "/Textures/Interface/Actions/actions_fakemindshield.rsi/icon-on.png",
            Loc.GetString("mc-armor-examinable-verb-message")
        );
    }

    private FormattedMessage GetArmorExamine(EntityUid entityUid)
    {
        var ev = new MCArmorGetEvent(SlotFlags.OUTERCLOTHING | SlotFlags.INNERCLOTHING, new MCArmorDefinition());
        RaiseLocalEvent(entityUid, ref ev);

        var msg = new FormattedMessage();
        msg.AddMarkupOrThrow(Loc.GetString("mc-armor-examine-title"));

        var armorRatings = new[]
        {
            (Loc.GetString("mc-armor-melee"), ev.ArmorDefinition.Melee),
            (Loc.GetString("mc-armor-bullet"), ev.ArmorDefinition.Bullet),
            (Loc.GetString("mc-armor-laser"), ev.ArmorDefinition.Laser),
            (Loc.GetString("mc-armor-energy"), ev.ArmorDefinition.Energy),
            (Loc.GetString("mc-armor-bomb"), ev.ArmorDefinition.Bomb),
            (Loc.GetString("mc-armor-bio"), ev.ArmorDefinition.Bio),
            (Loc.GetString("mc-armor-fire"), ev.ArmorDefinition.Fire),
            (Loc.GetString("mc-armor-acid"), ev.ArmorDefinition.Acid),
        };

        foreach (var (text, value) in armorRatings)
        {
            if (value == 0)
                continue;

            msg.PushNewline();
            msg.AddMarkupOrThrow(Loc.GetString(
                "mc-armor-examine-armor",
                ("text", text),
                ("value", value)
            ));
        }

        return msg;
    }
}
