using Content.Shared._RMC14.Actions;
using Content.Shared._RMC14.Atmos;
using Content.Shared._RMC14.Weapons.Ranged;
using Content.Shared.Actions;
using Content.Shared.Interaction.Events;
using Content.Shared.Jittering;
using Content.Shared.Projectiles;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Physics.Events;
using Robust.Shared.Timing;

namespace Content.Shared._MC.Xeno.Abilities.Evasion;

public sealed class MCXenoEvasionSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedJitteringSystem _jittering = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly SharedRMCActionsSystem _rmcActions = default!;

    private EntityQuery<RMCBulletComponent> _bulletQuery;
    private EntityQuery<ProjectileComponent> _projectileQuery;
    private EntityQuery<IgniteOnProjectileHitComponent> _igniteProjectileQuery;
    private EntityQuery<MCXenoEvasionComponent> _evasionQuery;

    public override void Initialize()
    {
        base.Initialize();

        _bulletQuery = GetEntityQuery<RMCBulletComponent>();
        _projectileQuery = GetEntityQuery<ProjectileComponent>();
        _igniteProjectileQuery = GetEntityQuery<IgniteOnProjectileHitComponent>();
        _evasionQuery = GetEntityQuery<MCXenoEvasionComponent>();

        SubscribeLocalEvent<MCXenoEvasionComponent, MCXenoEvasionActionEvent>(OnAction);

        SubscribeLocalEvent<MCXenoEvaderComponent, ComponentStartup>(OnEvaderStartup);
        SubscribeLocalEvent<MCXenoEvaderComponent, ComponentShutdown>(OnEvaderShutdown);
        SubscribeLocalEvent<MCXenoEvaderComponent, AttackAttemptEvent>(OnEvaderAttackAttempt);
        SubscribeLocalEvent<MCXenoEvaderComponent, PreventCollideEvent>(OnEvaderPreventCollide);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<MCXenoEvaderComponent>();
        while (query.MoveNext(out var entityUid, out var evaderComponent))
        {
            var seconds = (int) Math.Round((evaderComponent.EndTime - _timing.CurTime).TotalSeconds);
            _appearance.SetData(entityUid, EvasionVisuals.Visuals, seconds);

            if (_timing.CurTime < evaderComponent.EndTime)
                continue;

            RemCompDeferred<MCXenoEvaderComponent>(entityUid);
        }
    }

    private void OnAction(Entity<MCXenoEvasionComponent> entity, ref MCXenoEvasionActionEvent args)
    {
        if (args.Handled)
            return;

        if (!_rmcActions.TryUseAction(entity, args.Action, entity))
            return;

        args.Handled = true;

        var added = EnsureComp<MCXenoEvaderComponent>(entity, out var evaderComponent);
        try
        {
            if (added)
            {
                evaderComponent.EndTime += entity.Comp.Duration;
                return;
            }

            evaderComponent.EndTime = _timing.CurTime + entity.Comp.Duration;

        }
        finally
        {
            Dirty(entity, evaderComponent);
        }
    }

    private void OnEvaderStartup(Entity<MCXenoEvaderComponent> entity, ref ComponentStartup args)
    {
        _appearance.SetData(entity, EvasionLayer.Base, true);
    }

    private void OnEvaderShutdown(Entity<MCXenoEvaderComponent> entity, ref ComponentShutdown args)
    {
        _appearance.SetData(entity, EvasionLayer.Base, false);
    }

    private void OnEvaderAttackAttempt(Entity<MCXenoEvaderComponent> entity, ref AttackAttemptEvent args)
    {
        if (args.Cancelled)
            return;

        // args.Cancel();
    }

    private void OnEvaderPreventCollide(Entity<MCXenoEvaderComponent> entity, ref PreventCollideEvent args)
    {
        if (args.Cancelled || !_bulletQuery.HasComp(args.OtherEntity) || !_evasionQuery.TryComp(entity, out var evasionComponent))
            return;

        args.Cancelled = true;

        var damage = GetDamage(args.OtherEntity);
        entity.Comp.Stacks = Math.Max(0, entity.Comp.Stacks + damage);
        Dirty(entity);

        if (entity.Comp.Stacks >= evasionComponent.RefreshThreshold)
            RefreshAction(entity);

        if (evasionComponent.EvadeSound is not null)
            _audio.PlayPredicted(evasionComponent.EvadeSound, entity, entity);

        _jittering.DoJitter(entity, TimeSpan.FromSeconds(0.5), true, frequency: 6);
    }

    private void RefreshAction(EntityUid uid)
    {
        foreach (var action in _rmcActions.GetActionsWithEvent<MCXenoEvasionActionEvent>(uid))
        {
            _actions.ClearCooldown((action, action));
        }
    }

    private float GetDamage(EntityUid uid)
    {
        var damage = _projectileQuery.CompOrNull(uid)?.Damage.GetTotal().Float() ?? 0;
        return _igniteProjectileQuery.HasComp(uid) ? -damage : damage;
    }
}
