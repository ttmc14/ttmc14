using Content.Shared._CE.ZLevels.Core.Components;
using Content.Shared._CE.ZLevels.Core.EntitySystems;
using Content.Shared._MC.Xeno.Abilities.Warlock.Antigravity.Components;
using Content.Shared._MC.Xeno.Abilities.Warlock.Antigravity.Events;
using Content.Shared.Gravity;
using Content.Shared.Movement.Components;

namespace Content.Shared._MC.Xeno.Abilities.Warlock.Antigravity;

public sealed class MCXenoAntigravitySystem : MCXenoAbilitySystem
{
    [Dependency] private readonly CESharedZLevelsSystem _zLevels = null!;

    public override void Initialize()
    {
        SubscribeLocalEvent<MCXenoAntigravityComponent, MCXenoAntigravityMoveUpActionEvent>(OnActionUp);
        SubscribeLocalEvent<MCXenoAntigravityComponent, MCXenoAntigravityMoveDownActionEvent>(OnActionDown);

        SubscribeLocalEvent<MCXenoAntigravityComponent, MCXenoAntigravityActionEvent>(OnAction);

        SubscribeLocalEvent<MCXenoAntigravityComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<MCXenoAntigravityComponent, IsWeightlessEvent>(OnGetWeightless);
        SubscribeLocalEvent<MCXenoAntigravityComponent, CECheckGravityEvent>(OnGravity);
        SubscribeLocalEvent<MCXenoAntigravityComponent, CEGetZVelocityEvent>(OnGetZVelocity);
    }

    private void OnStartup(Entity<MCXenoAntigravityComponent> entity, ref ComponentStartup args)
    {
        EnsureComp<MovementAlwaysTouchingComponent>(entity);

        var physics = EnsureComp<CEZPhysicsComponent>(entity);

        physics.VelocityRaiseEvent = true;
        Dirty(entity, physics);
    }

    private void OnGetWeightless(Entity<MCXenoAntigravityComponent> entity, ref IsWeightlessEvent args)
    {
        if (!entity.Comp.Active)
            return;

        args.IsWeightless = false;
        args.Handled = true;
    }

    private void OnActionUp(Entity<MCXenoAntigravityComponent> entity, ref MCXenoAntigravityMoveUpActionEvent args)
    {
        if (args.Handled)
            return;

        var map = Transform(entity).MapUid;
        if (map is null)
            return;

        if (!entity.Comp.Active)
            return;

        if (!_zLevels.TryMapUp(map.Value, out var mapAbove))
            return;

        entity.Comp.TargetMapHeight = mapAbove.Comp.Depth;
        DirtyField(entity, entity.Comp, nameof(MCXenoAntigravityComponent.TargetMapHeight));

        args.Handled = true;
    }

    private void OnActionDown(Entity<MCXenoAntigravityComponent> entity, ref MCXenoAntigravityMoveDownActionEvent args)
    {
        if (args.Handled)
            return;

        var map = Transform(entity).MapUid;
        if (map is null)
            return;

        if (!entity.Comp.Active)
            return;

        if (!_zLevels.TryMapDown(map.Value, out var mapBelow))
            return;

        entity.Comp.TargetMapHeight = mapBelow.Comp.Depth;
        DirtyField(entity, entity.Comp, nameof(MCXenoAntigravityComponent.TargetMapHeight));

        args.Handled = true;
    }

    private static void OnGetZVelocity(Entity<MCXenoAntigravityComponent> entity, ref CEGetZVelocityEvent args)
    {
        if (!entity.Comp.Active)
            return;

        var zPhys = args.Target.Comp;
        var currentPos = zPhys.CurrentZLevel + zPhys.LocalPosition;
        var targetPos = entity.Comp.TargetMapHeight + 0.2f;
        var currentVelocity = zPhys.Velocity;

        var distanceToTarget = targetPos - currentPos;

        var targetVelocity = Math.Clamp(distanceToTarget * entity.Comp.Speed, -entity.Comp.Speed, entity.Comp.Speed);
        var velocityDelta = targetVelocity - currentVelocity;

        var upperBound = entity.Comp.TargetMapHeight + 0.9f;
        var lowerBound = entity.Comp.TargetMapHeight + 0.1f;

        var newVelocity = currentVelocity + velocityDelta;
        var nextPos = currentPos + newVelocity;

        if (nextPos > upperBound)
        {
            var maxAllowedVelocity = upperBound - currentPos;
            velocityDelta = maxAllowedVelocity - currentVelocity;
        }
        else if (nextPos < lowerBound)
        {
            var maxAllowedVelocity = lowerBound - currentPos;
            velocityDelta = maxAllowedVelocity - currentVelocity;
        }

        args.VelocityDelta = velocityDelta;
    }

    private static void OnGravity(Entity<MCXenoAntigravityComponent> entity, ref CECheckGravityEvent args)
    {
        if (!entity.Comp.Active)
            return;

        args.Gravity = 0;
    }

    private void OnAction(Entity<MCXenoAntigravityComponent> entity, ref MCXenoAntigravityActionEvent args)
    {
        if (args.Handled)
            return;

        if (!TryUseAction(entity, args.Action))
            return;

        entity.Comp.Active = true;
        DirtyField(entity, entity.Comp, nameof(MCXenoAntigravityComponent.Active));

        _zLevels.UpdateGravityState(entity.Owner);

        args.Handled = true;
    }
}
