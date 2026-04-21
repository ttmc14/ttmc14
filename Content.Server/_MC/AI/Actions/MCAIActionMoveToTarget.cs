using Content.Server.NPC.Components;
using Content.Server.NPC.Pathfinding;
using Content.Server.NPC.Systems;
using Content.Shared._MC.AI;
using Content.Shared._MC.AI.Events;
using Content.Shared._MC.AI.Modules;
using Robust.Shared.Map;

namespace Content.Server._MC.AI.Actions;

public sealed partial class MCAIActionMoveToTarget : MCAIAction<MCAIActionMoveToTarget>
{
    [DataField]
    public string TargetKey = string.Empty;

    [DataField]
    public float Range = 1.15f;

    [DataField]
    public float ReregisterThreshold = 1.15f;
}

public sealed partial class MCAIActionMoveToTargetSystem : MCAIActionSystem<MCAIActionMoveToTarget>
{
    [Dependency] private readonly NPCSteeringSystem _steering = null!;

    private EntityQuery<NPCSteeringComponent> _steeringQuery;

    public override void Initialize()
    {
        base.Initialize();

        _steeringQuery = GetEntityQuery<NPCSteeringComponent>();
    }

    protected override void OnActionStartup(Entity<MCAIAgentComponent> entity, ref MCAIActionStartupEvent<MCAIActionMoveToTarget> args)
    {
        if (!GetCoordinates(entity.Comp.Memory, args.Action.TargetKey, out var targetCoordinates))
            return;

        var component = _steering.Register(entity, targetCoordinates);
        component.Range = args.Action.Range;
        component.Flags = PathFlags.Access | PathFlags.Climbing | PathFlags.Interact | PathFlags.Smashing;
    }

    protected override MCAIActionStatus OnActionUpdate(Entity<MCAIAgentComponent> entity, MCAIActionMoveToTarget action, float frameTime)
    {
        if (!GetCoordinates(entity.Comp.Memory, action.TargetKey, out var targetCoordinates))
            return MCAIActionStatus.Failed;

        if (!_steeringQuery.TryComp(entity, out var steering))
            return MCAIActionStatus.Failed;

        if (steering.Coordinates.TryDistance(EntityManager, targetCoordinates, out var delta) && delta > action.ReregisterThreshold)
        {
            var component = _steering.Register(entity, targetCoordinates);
            component.Range = action.Range;
            component.Flags = PathFlags.Access | PathFlags.Climbing | PathFlags.Interact | PathFlags.Smashing;
        }

        return steering.Status switch
        {
            SteeringStatus.InRange => MCAIActionStatus.Finished,
            SteeringStatus.NoPath => MCAIActionStatus.Failed,
            _ => MCAIActionStatus.Running,
        };
    }

    protected override void OnActionShutdown(Entity<MCAIAgentComponent> ent, ref MCAIActionShutdownEvent<MCAIActionMoveToTarget> args)
    {
        _steering.Unregister(ent);
    }

    private bool GetCoordinates(MCAIMemory memory, string targetKey, out EntityCoordinates coordinates)
    {
        coordinates = EntityCoordinates.Invalid;

        if (memory.ContainerTryGet<EntityUid>(targetKey, out var targetUid))
        {
            coordinates = Transform(targetUid).Coordinates;
            return true;
        }

        if (memory.ContainerTryGet<EntityCoordinates>(targetKey, out var entityCoordinates))
        {
            coordinates = entityCoordinates;
            return true;
        }

        return false;
    }
}
