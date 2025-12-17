using Content.Shared._RMC14.Actions;
using Content.Shared.Atmos.Components;
using Content.Shared.Damage;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Movement.Pulling.Components;
using Content.Shared.Movement.Pulling.Systems;
using Robust.Shared.Network;
using Robust.Shared.Timing;

namespace Content.Shared._MC.Xeno.Abilities.Rewind;

public abstract class MCSharedXenoRewindSystem : EntitySystem
{
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly RMCActionsSystem _rmcActions = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly PullingSystem _pulling = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MCXenoRewindComponent, MCXenoRewindActionEvent>(OnAction);

        SubscribeLocalEvent<MCXenoRewindTargetComponent, MobStateChangedEvent>(OnTagetChangedState);
        SubscribeLocalEvent<MCXenoRewindTargetComponent, ComponentStartup>(OnTagetStartup);
        SubscribeLocalEvent<MCXenoRewindTargetComponent, ComponentShutdown>(OnTagetShutdown);
    }

    private void OnAction(Entity<MCXenoRewindComponent> entity, ref MCXenoRewindActionEvent args)
    {
        if (args.Handled)
            return;

        var target = args.Target;
        var distance = (Transform(entity).Coordinates - Transform(target).Coordinates).Position.LengthSquared();
        if (distance > entity.Comp.Range * entity.Comp.Range)
            return;

        if (!HasComp<MobStateComponent>(target))
            return;

        if (HasComp<MCXenoRewindTargetComponent>(target))
            return;

        if (_mobState.IsDead(target))
            return;

        if (!_rmcActions.TryUseAction(entity, args.Action, entity))
            return;

        args.Handled = true;

        if (_net.IsClient)
            return;

        EnsureComp<MCXenoRewindTargetComponent>(target);
        Timer.Spawn(entity.Comp.Delay,
            () =>
            {
                RemCompDeferred<MCXenoRewindTargetComponent>(target);
            }
        );
    }

    private void OnTagetChangedState(Entity<MCXenoRewindTargetComponent> entity, ref MobStateChangedEvent args)
    {
        if (args.NewMobState != MobState.Dead)
            return;

        entity.Comp.Canceled = true;
        Dirty(entity);
    }

    private void OnTagetStartup(Entity<MCXenoRewindTargetComponent> entity, ref ComponentStartup _)
    {
        if (entity.Comp.Canceled)
            return;

        entity.Comp.Position = Transform(entity).Coordinates;

        if (TryComp<DamageableComponent>(entity, out var damageableComponent))
            entity.Comp.Damage = new DamageSpecifier(damageableComponent.Damage);

        if (TryComp<FlammableComponent>(entity, out var flammableComponent))
            entity.Comp.FireStacks = flammableComponent.FireStacks;

        Dirty(entity);
    }

    protected virtual void OnTagetShutdown(Entity<MCXenoRewindTargetComponent> entity, ref ComponentShutdown _)
    {
        _transform.SetCoordinates(entity, entity.Comp.Position);

        if (TryComp<DamageableComponent>(entity, out var damageableComponent) && entity.Comp.Damage is not null)
             _damageable.SetDamage(entity, damageableComponent, entity.Comp.Damage);

        if (TryComp<PullableComponent>(entity, out var pullable) && pullable.BeingPulled)
            _pulling.TryStopPull(entity, pullable);

        if (TryComp<PullerComponent>(entity, out var pullerComp) && TryComp<PullableComponent>(pullerComp.Pulling, out var subjectPulling))
            _pulling.TryStopPull(pullerComp.Pulling.Value, subjectPulling);
    }
}
