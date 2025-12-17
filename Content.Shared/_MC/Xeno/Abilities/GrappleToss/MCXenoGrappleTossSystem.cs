using Content.Shared._RMC14.Actions;
using Content.Shared._RMC14.Pulling;
using Content.Shared._RMC14.Slow;
using Content.Shared._RMC14.Xenonids.Hive;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Mobs.Components;
using Content.Shared.Movement.Pulling.Components;
using Content.Shared.Stunnable;
using Content.Shared.Throwing;

namespace Content.Shared._MC.Xeno.Abilities.GrappleToss;

public sealed class MCXenoGrappleTossSystem : EntitySystem
{
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly SharedStunSystem _stun = default!;
    [Dependency] private readonly ThrowingSystem _throwing = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly RMCActionsSystem _rmcActions = default!;
    [Dependency] private readonly RMCPullingSystem _rmcPulling = default!;
    [Dependency] private readonly RMCSlowSystem _rmcSlow = default!;
    [Dependency] private readonly SharedXenoHiveSystem _xenoHive = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MCXenoGrappleTossComponent, MCXenoGrappleTossActionEvent>(OnAction);
    }

    private void OnAction(Entity<MCXenoGrappleTossComponent> entity, ref MCXenoGrappleTossActionEvent args)
    {
        if (args.Handled)
            return;

        if (!TryComp<PullerComponent>(entity, out var pullerComponent))
            return;

        if (pullerComponent.Pulling is not { } targetEntity)
            return;

        if (!HasComp<MobStateComponent>(targetEntity))
            return;

        if (!_rmcActions.TryUseAction(entity, args.Action, entity))
            return;

        args.Handled = true;

        if (!_xenoHive.FromSameHive(entity.Owner, targetEntity))
        {
            _rmcSlow.TrySlowdown(targetEntity, entity.Comp.SlowdownDuration);
            _stun.TryParalyze(targetEntity, entity.Comp.ParalyzeDuration, true);
        }

        var origin = _transform.GetMapCoordinates(entity);
        var delta = (args.Target.Position - origin.Position).Normalized() * entity.Comp.Distance;

        _rmcPulling.TryStopAllPullsFromAndOn(targetEntity);
        _throwing.TryThrow(targetEntity, delta, entity.Comp.Speed);
    }
}
