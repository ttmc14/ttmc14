using Content.Shared._MC.Armor.Core.Components;
using Content.Shared._MC.Armor.Core.Events;
using Content.Shared._MC.Xeno.Sunder;
using Content.Shared._RMC14.Armor;
using Content.Shared.Damage;
using Content.Shared.Examine;
using Content.Shared.Explosion;
using Content.Shared.Inventory;
using Content.Shared.Tag;
using Content.Shared.Verbs;

namespace Content.Shared._MC.Armor.Core;

public sealed partial class MCArmorSystem : EntitySystem
{
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

    private void OnDamageModify(Entity<MCArmorComponent> entity, ref DamageModifyEvent args)
    {
        DamageModify(entity, ref args);
    }

    private void DamageModify(EntityUid entityUid, ref DamageModifyEvent args)
    {
        if (args.Tool is not { } tool)
            return;

        var piercingEvent = new CMGetArmorPiercingEvent(tool);
        RaiseLocalEvent(args.Tool.Value, ref piercingEvent);

        var sunder = _mcXenoSunder.GetSunder(entityUid);
        var (soft, hard) = GetArmorWithType(entityUid, tool);

        args.Damage *= ArmorToValue(soft, hard, args.ArmorPiercing + piercingEvent.Piercing, sunder, args.Damage.GetTotal().Float());
    }
}
