using Content.Shared._MC.Xeno.Hive.Systems;
using Content.Shared._MC.Xeno.Weeds;
using Content.Shared._RMC14.Damage;
using Content.Shared._RMC14.Xenonids.Pheromones;
using Content.Shared._RMC14.Xenonids.Rest;
using Content.Shared._RMC14.Xenonids.Weeds;
using Content.Shared.Damage;
using Content.Shared.Damage.Prototypes;
using Content.Shared.FixedPoint;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Systems;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Shared._MC.Xeno.Heal;

public sealed class MCXenoHealSystem : MCEntitySystemSingleton<MCXenoHealSingletonComponent>
{
    private const float XenoRestingHeal = 1;
    private const float UpdateFrequency = 1;

    private static readonly ProtoId<DamageGroupPrototype> BruteGroup = "MCBrute";
    private static readonly ProtoId<DamageGroupPrototype> BurnGroup = "MCBurn";
    private static readonly ProtoId<DamageGroupPrototype> ToxinGroup = "MCToxin";

    [Dependency] private readonly IGameTiming _timing = null!;

    [Dependency] private readonly MobThresholdSystem _mobThresholds = null!;
    [Dependency] private readonly DamageableSystem _damageable = null!;

    [Dependency] private readonly MCSharedXenoHiveSystem _mcXenoHive = null!;
    [Dependency] private readonly SharedRMCDamageableSystem _rmcDamageable = null!;

    private EntityQuery<DamageableComponent> _damageableQuery;
    private EntityQuery<AffectableByWeedsComponent> _rmcAffectableQuery;
    private EntityQuery<XenoRecoveryPheromonesComponent> _rmcXenoRecoveryPheromonesQuery;
    private EntityQuery<MCXenoWeedsRegenerationComponent> _mcWeedsRegenerationQuery;
    private EntityQuery<MCXenoHealCacheComponent> _mcXenoHealthCacheQuery;

    public override void Initialize()
    {
        base.Initialize();

        _damageableQuery = GetEntityQuery<DamageableComponent>();
        _rmcAffectableQuery = GetEntityQuery<AffectableByWeedsComponent>();
        _rmcXenoRecoveryPheromonesQuery = GetEntityQuery<XenoRecoveryPheromonesComponent>();
        _mcWeedsRegenerationQuery = GetEntityQuery<MCXenoWeedsRegenerationComponent>();
        _mcXenoHealthCacheQuery = GetEntityQuery<MCXenoHealCacheComponent>();

        SubscribeLocalEvent<MCXenoHealComponent, DamageChangedEvent>(OnHealDamageChanged);
        SubscribeLocalEvent<XenoRecoveryPheromonesComponent, ComponentStartup>(OnRecoverPheromonesStartup);
    }

    private void OnRecoverPheromonesStartup(Entity<XenoRecoveryPheromonesComponent> entity, ref ComponentStartup args)
    {
        if (!TryComp<MCXenoHealComponent>(entity, out var xenoHealComponent))
            return;

        xenoHealComponent.RegenerationTimeNext = _timing.CurTime + TimeSpan.FromSeconds(UpdateFrequency);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<MCXenoHealComponent>();
        while (query.MoveNext(out var uid, out var xenoHealComponent))
        {
            if (_timing.CurTime < xenoHealComponent.RegenerationTimeNext)
                continue;

            xenoHealComponent.RegenerationTimeNext = _timing.CurTime + TimeSpan.FromSeconds(UpdateFrequency);

            var affectable = _rmcAffectableQuery.CompOrNull(uid);
            if (!affectable?.OnXenoWeeds ?? false)
                continue;

            var resting = HasComp<XenoRestingComponent>(uid);
            var multiplier = resting ? xenoHealComponent.RestingMultiplier : xenoHealComponent.StandMultiplier;

            multiplier *= GetWeedsHealthMultiplier(uid);
            multiplier *= GetRulerHealthMultiplier(uid);

            HealWounds((uid, xenoHealComponent), multiplier, powerScaling: true);
        }
    }

    private void OnHealDamageChanged(Entity<MCXenoHealComponent> entity, ref DamageChangedEvent args)
    {
        if (!args.DamageIncreased)
            return;

        entity.Comp.RegenerationPower = 0;
        DirtyField(entity, entity.Comp, nameof(MCXenoHealComponent.RegenerationPower));

        if (_rmcXenoRecoveryPheromonesQuery.HasComponent(entity))
            return;

        entity.Comp.RegenerationTimeNext = _timing.CurTime + entity.Comp.RegenerationDelay;
        DirtyField(entity, entity.Comp, nameof(MCXenoHealComponent.RegenerationTimeNext));
    }

    public float HealWounds(Entity<MCXenoHealComponent?> entity, float multiplier = XenoRestingHeal, bool powerScaling = false, float baseHeal = 1, float maxHealthMultiplier = 0.0375f)
    {
        if (!Resolve(entity, ref entity.Comp))
            return 0;

        var recoveryAura = GetRecoveryAura(entity);
        var hasAura = recoveryAura == 0;
        var maxHealth = GetMaxHealth(entity);

        // 1 damage + 3.75% max health, with scaling power.
        // +1% max health per recovery level, up to +5%
        var amount = baseHeal + maxHealth * maxHealthMultiplier + recoveryAura * maxHealth * 0.01f;

        if (powerScaling)
        {
            var power = float.Min(entity.Comp.RegenerationPower + entity.Comp.RegenerationRampAmount * 20 , 1);
            if (hasAura)
                power = float.Clamp(power + entity.Comp.RegenerationPower * 30, 0, 1);

            amount *= power;

            entity.Comp.RegenerationPower = power;
        }

        amount *= multiplier * Inst.Comp.GlobalHealBuff;
        Heal(entity, amount);

        return amount;
    }

    public void Heal(EntityUid uid, float amount)
    {
        var damage = _rmcDamageable.DistributeDamage(uid, BruteGroup, amount);
        var totalHeal = damage.GetTotal();
        var leftover = amount - totalHeal;
        if (leftover > FixedPoint2.Zero)
            damage = _rmcDamageable.DistributeDamage(uid, BurnGroup, leftover, damage);
        _damageable.TryChangeDamage(uid, -damage, true);
    }

    public float GetHealth(EntityUid uid)
    {
        return _damageableQuery.TryComp(uid, out var damageableComponent)
            ? GetHealthAlive(uid) - damageableComponent.TotalDamage.Float()
            : 0;
    }

    public float GetRecoveryAura(EntityUid uid)
    {
        return _rmcXenoRecoveryPheromonesQuery.CompOrNull(uid)?.Multiplier.Float() ?? 0;
    }

    public float GetHealthAlive(EntityUid uid)
    {
        return _mcXenoHealthCacheQuery.TryGetComponent(uid, out var mcXenoHealthCacheComponent)
            ? mcXenoHealthCacheComponent.Health
            : HealCache(uid).Health;
    }

    public float GetMaxHealth(EntityUid uid)
    {
        return _mcXenoHealthCacheQuery.TryGetComponent(uid, out var mcXenoHealthCacheComponent)
            ? mcXenoHealthCacheComponent.MaxHealth
            : HealCache(uid).MaxHealth;
    }

    private MCXenoHealCacheComponent HealCache(EntityUid uid)
    {
        var healCacheComponent = EnsureComp<MCXenoHealCacheComponent>(uid);
        healCacheComponent.Health = GetHealthThreshold(uid);
        healCacheComponent.MaxHealth = GetMaxHealthThreshold(uid);
        Dirty(uid, healCacheComponent);
        return healCacheComponent;
    }

    private float GetHealthThreshold(EntityUid uid)
    {
        if (_mobThresholds.TryGetThresholdForState(uid, MobState.Critical, out var criticalThreshold))
            return criticalThreshold.Value.Float();

        return 0;
    }

    private float GetMaxHealthThreshold(EntityUid uid)
    {
        if (_mobThresholds.TryGetThresholdForState(uid, MobState.Dead, out var deadThreshold))
            return deadThreshold.Value.Float();

        if (_mobThresholds.TryGetThresholdForState(uid, MobState.Critical, out var criticalThreshold))
            return criticalThreshold.Value.Float();

        return 0;
    }

    private float GetRulerHealthMultiplier(EntityUid uid)
    {
        return _mcXenoHive.HiveMemberHasRuler(uid) ? 1 : 0.5f;
    }

    private float GetWeedsHealthMultiplier(Entity<AffectableByWeedsComponent?> entity)
    {
        return GetWeedsRegenerationComponent(entity)?.HealthModifier ?? 1f;
    }

    private MCXenoWeedsRegenerationComponent? GetWeedsRegenerationComponent(Entity<AffectableByWeedsComponent?> entity)
    {
        entity.Comp ??= _rmcAffectableQuery.CompOrNull(entity);
        return entity.Comp?.LastWeedsEntity is null
            ? null
            : _mcWeedsRegenerationQuery.CompOrNull(entity.Comp.LastWeedsEntity.Value);
    }
}

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MCXenoHealSingletonComponent : Component
{
    [DataField, AutoNetworkedField]
    public float GlobalHealBuff = 1;
}
