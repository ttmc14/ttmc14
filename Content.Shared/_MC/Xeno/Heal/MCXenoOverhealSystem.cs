using Content.Shared._MC.Aura;
using Content.Shared._MC.Damage;
using Content.Shared._MC.Xeno.Hive.Systems.Main;
using Content.Shared._RMC14.Damage;
using Content.Shared._RMC14.Xenonids;
using Content.Shared.Coordinates;
using Content.Shared.Damage;
using Content.Shared.Damage.Prototypes;
using Content.Shared.Examine;
using Content.Shared.FixedPoint;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Systems;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;

namespace Content.Shared._MC.Xeno.Heal;

public sealed class MCXenoOverhealSystem : EntitySystem
{
    private const string AuraLayer = "overheal";

    [Dependency] private readonly MCAuraSystem _aura = null!;
    [Dependency] private readonly MCXenoHealSystem _heal = null!;
    [Dependency] private readonly INetManager _net = null!;
    [Dependency] private readonly EntityLookupSystem _lookup = null!;
    [Dependency] private readonly MCSharedXenoHiveSystem _hive = null!;
    [Dependency] private readonly MobStateSystem _mobState = null!;

    /// <summary>
    /// Recipients found by the current area heal, reused between pulses.
    /// </summary>
    private readonly HashSet<Entity<MCXenoOverhealComponent>> _nearby = [];

    /// <summary>
    /// Damage types affected by overheal, psychic links and carapace hardening.
    /// </summary>
    internal static readonly ProtoId<DamageTypePrototype>[] PhysicalDamageTypes = [MCDamageableSystem.DamageBruteId, MCDamageableSystem.DamageBurnId];

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<MCXenoOverhealComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<MCXenoOverhealComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<MCXenoOverhealComponent, ExaminedEvent>(OnExamine);
        SubscribeLocalEvent<MCXenoOverhealComponent, DamageModifyAfterResistEvent>(OnDamage);
        SubscribeLocalEvent<MCXenoOverhealComponent, MobStateChangedEvent>(OnStateChanged);
    }

    /// <summary>
    /// Heals wounds and stores the specified fraction of unused healing as overheal.
    /// </summary>
    public void Restore(EntityUid target, float amount, float excessMultiplier = 1, EntProtoId? effect = null)
    {
        var before = _heal.GetHealth(target);
        _heal.Heal(target, amount);
        var excess = amount - (_heal.GetHealth(target) - before);
        AddOverheal(target, excess * excessMultiplier);
        if (_net.IsServer && effect is { } visual)
        {
            SpawnAttachedTo(visual, target.ToCoordinates());
        }
    }

    /// <summary>
    /// Applies healing to other living members of the source hive inside the radius.
    /// </summary>
    public void RestoreNearby(EntityUid source, float range, float amount, float excessMultiplier, EntProtoId effect)
    {
        _nearby.Clear();
        _lookup.GetEntitiesInRange(Transform(source).Coordinates, range, _nearby, LookupFlags.Uncontained);
        foreach (var target in _nearby)
        {
            if (target.Owner != source && !_mobState.IsDead(target.Owner) && _hive.FromSameHive(source, target.Owner))
            {
                Restore(target.Owner, amount, excessMultiplier, effect);
            }
        }
    }

    /// <summary>
    /// Adds to the target's reserve up to the maximum configured for its caste.
    /// </summary>
    public void AddOverheal(EntityUid target, float amount)
    {
        if (amount <= 0 || !TryComp<MCXenoOverhealComponent>(target, out var comp) || comp.Amount >= comp.Maximum)
            return;

        comp.Amount = FixedPoint2.Min(comp.Maximum, comp.Amount + amount);
        Dirty(target, comp);
        UpdateAura((target, comp));
    }

    private void OnStartup(Entity<MCXenoOverhealComponent> ent, ref ComponentStartup args)
    {
        UpdateAura(ent);
    }

    private void OnShutdown(Entity<MCXenoOverhealComponent> ent, ref ComponentShutdown args)
    {
        _aura.Remove(ent, AuraLayer);
    }

    private void UpdateAura(Entity<MCXenoOverhealComponent> ent)
    {
        if (ent.Comp.Amount <= 0 || ent.Comp.Maximum <= 0)
        {
            _aura.Remove(ent, AuraLayer);
            return;
        }

        _aura.Set(ent, AuraLayer, ent.Comp.Aura, ent.Comp.Amount.Float() / ent.Comp.Maximum.Float());
    }

    private void OnExamine(Entity<MCXenoOverhealComponent> ent, ref ExaminedEvent args)
    {
        if (ent.Comp.Amount > 0 && HasComp<XenoComponent>(args.Examiner))
        {
            args.PushMarkup(Loc.GetString("mc-xeno-gorger-overheal", ("amount", ent.Comp.Amount.Float()), ("maximum", ent.Comp.Maximum.Float())));
        }
    }

    private void OnStateChanged(Entity<MCXenoOverhealComponent> ent, ref MobStateChangedEvent args)
    {
        if (args.NewMobState == MobState.Dead)
        {
            ent.Comp.Amount = 0;
            Dirty(ent);
            UpdateAura(ent);
        }
    }

    private void OnDamage(Entity<MCXenoOverhealComponent> ent, ref DamageModifyAfterResistEvent args)
    {
        if (ent.Comp.Amount <= 0)
            return;

        var remaining = new DamageSpecifier(args.Damage);
        var previous = ent.Comp.Amount;
        foreach (var type in PhysicalDamageTypes)
        {
            if (!remaining.DamageDict.TryGetValue(type, out var amount) || amount <= 0)
                continue;

            var absorbed = FixedPoint2.Min(ent.Comp.Amount, amount);
            ent.Comp.Amount -= absorbed;
            remaining.DamageDict[type] = amount - absorbed;
        }

        if (previous == ent.Comp.Amount)
            return;

        args.Damage = remaining;
        Dirty(ent);
        UpdateAura(ent);
    }
}
