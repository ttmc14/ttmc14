using Content.Shared._MC.Xeno.Constructions.Weeds;
using Content.Shared._RMC14.Xenonids.Pheromones;
using Content.Shared._RMC14.Xenonids.Rest;
using Content.Shared._RMC14.Xenonids.Weeds;
using Content.Shared.Alert;
using Content.Shared.Projectiles;
using Content.Shared.Rounding;
using Robust.Shared.Timing;

namespace Content.Shared._MC.Xeno.Sunder;

public sealed class MCXenoSunderSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = null!;
    [Dependency] private readonly AlertsSystem _alerts = null!;

    private EntityQuery<MCXenoSunderComponent> _sunderQuery;
    private EntityQuery<XenoRestingComponent> _xenoRestingQuery;
    private EntityQuery<MCXenoWeedsRegenerationComponent> _weedsRegenerationQuery;
    private EntityQuery<XenoRecoveryPheromonesComponent> _xenoRecoveryPheromones;

    public override void Initialize()
    {
        base.Initialize();

        _sunderQuery = GetEntityQuery<MCXenoSunderComponent>();
        _xenoRestingQuery = GetEntityQuery<XenoRestingComponent>();
        _weedsRegenerationQuery = GetEntityQuery<MCXenoWeedsRegenerationComponent>();
        _xenoRecoveryPheromones = GetEntityQuery<XenoRecoveryPheromonesComponent>();

        SubscribeLocalEvent<MCXenoSunderComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<MCXenoSunderComponent, ComponentShutdown>(OnShutdown);

        SubscribeLocalEvent<MCXenoSunderDamageOnHitComponent, ProjectileHitEvent>(OnProjectileHit);
    }

    private void OnStartup(Entity<MCXenoSunderComponent> entity, ref ComponentStartup args)
    {
        SetSunder((entity, entity), entity.Comp.Value);
    }

    private void OnShutdown(Entity<MCXenoSunderComponent> entity, ref ComponentShutdown args)
    {
        _alerts.ClearAlert(entity, entity.Comp.Alert);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<MCXenoSunderComponent, AffectableByWeedsComponent>();
        while (query.MoveNext(out var entity, out var sunderComponent, out var affectableByWeedsComponent))
        {
            if (_timing.CurTime < sunderComponent.NextRegenTime)
                continue;

            sunderComponent.NextRegenTime = _timing.CurTime + sunderComponent.RegenCooldown;
            DirtyField(entity, sunderComponent, nameof(MCXenoSunderComponent.NextRegenTime));

            if (!affectableByWeedsComponent.OnXenoWeeds)
                continue;

            var resting = _xenoRestingQuery.HasComp(entity);
            var weeds = affectableByWeedsComponent.LastWeedsEntity is null
                ? 1
                : _weedsRegenerationQuery.CompOrNull(affectableByWeedsComponent.LastWeedsEntity.Value)?.SunderModifier ?? 1;
            var pheromones = _xenoRecoveryPheromones.CompOrNull(entity)?.Multiplier.Float() ?? 0;

            RegenSunder((entity, sunderComponent), resting, weeds, pheromones);
        }
    }

    private void OnProjectileHit(Entity<MCXenoSunderDamageOnHitComponent> entity, ref ProjectileHitEvent args)
    {
        if (!_sunderQuery.TryComp(args.Target, out var sunderComponent))
            return;

        AddSunder(args.Target, -entity.Comp.Amount * sunderComponent.Multiplier);
    }

    public void RegenSunder(Entity<MCXenoSunderComponent?> entity, bool resting, float weeds, float pheromones)
    {
        if (!_sunderQuery.Resolve(entity, ref entity.Comp, logMissing: false))
            return;

        var restingModifier = resting ? 2 : 1;
        var regen = 0.5f * (0.5f * restingModifier * weeds * (1 + 0.1f * pheromones));

        AddSunder(entity, regen);
    }

    public void AddSunder(Entity<MCXenoSunderComponent?> entity, float amount)
    {
        if (!_sunderQuery.Resolve(entity, ref entity.Comp, logMissing: false))
            return;

        SetSunder(entity, entity.Comp.Value + amount);
    }

    public void SetSunder(Entity<MCXenoSunderComponent?> entity, float amount)
    {
        if (!_sunderQuery.Resolve(entity, ref entity.Comp, logMissing: false))
            return;

        entity.Comp.Value = Math.Clamp(amount, 0, 100);
        DirtyField(entity, entity.Comp, nameof(MCXenoSunderComponent.Value));

        var severity = ContentHelpers.RoundToLevels(entity.Comp.Value, 100, _alerts.GetMaxSeverity(entity.Comp.Alert) + 1);
        _alerts.ShowAlert(entity, entity.Comp.Alert, (short) severity, dynamicMessage: ((int) Math.Round(entity.Comp.Value)).ToString());
    }

    public float GetSunder(Entity<MCXenoSunderComponent?> entity)
    {
        if (!_sunderQuery.Resolve(entity, ref entity.Comp, logMissing: false))
            return 1f;

        return entity.Comp.Value * 0.01f;
    }
}
