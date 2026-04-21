using System.Numerics;
using Content.Shared._MC.AI;
using Content.Shared._MC.AI.Events;
using Content.Shared._MC.AI.Modules;
using Robust.Server.GameObjects;

namespace Content.Server._MC.AI;

public abstract class MCAISensorSystem<T> : EntitySystem where T : MCAISensor<T>
{
    [Dependency] protected readonly MCAISystem Ai = null!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MCAIAgentComponent, MCAISensorUpdate<T>>(HandleSensorUpdate);
    }

    private void HandleSensorUpdate(Entity<MCAIAgentComponent> entity, ref MCAISensorUpdate<T> args)
    {
        var state = OnSensorUpdate(entity, ref args);
        if (state is null)
            return;

        args.Memory.StateSet(args.Sensor.ConditionKey, state.Value);
    }

    protected virtual bool? OnSensorUpdate(Entity<MCAIAgentComponent> entity, ref MCAISensorUpdate<T> args)
    {
        return null;
    }
}

public abstract class MCAISensorHasComponentSystem<T, TComp> : MCAISensorSystem<T>
    where T : MCAISensor<T>
    where TComp : IComponent
{
    protected override bool? OnSensorUpdate(Entity<MCAIAgentComponent> entity, ref MCAISensorUpdate<T> args)
    {
        return HasComp<TComp>(entity);
    }
}


public abstract partial class MCAISensorNearestWithComponentSystem<T, TComp> : MCAISensorSystem<T>
    where T : MCAISensorNearestComponent<T>
    where TComp : IComponent
{
    [Dependency] private readonly EntityLookupSystem _lookup = null!;
    [Dependency] private readonly TransformSystem _transform = null!;

    protected sealed override bool? OnSensorUpdate(Entity<MCAIAgentComponent> entity, ref MCAISensorUpdate<T> args)
    {
        EntityUid? closestTarget = null;
        var closestDistance = float.MaxValue;

        var position = _transform.GetWorldPosition(entity);
        foreach (var inRangeEntity in  _lookup.GetEntitiesInRange<TComp>(Transform(entity).Coordinates, args.Sensor.VisionRadius, LookupFlags.Uncontained))
        {
            if (entity.Owner == inRangeEntity.Owner)
                continue;

            var targetWorldPos = _transform.GetWorldPosition(inRangeEntity);
            var distance = Vector2.Distance(position, targetWorldPos);

            if (distance >= closestDistance)
                continue;

            closestDistance = distance;
            closestTarget = inRangeEntity;
        }

        if (closestTarget is null)
            return false;

        entity.Comp.Memory.ContainerSet(args.Sensor.OutputTargetKey, closestTarget.Value);
        return true;
    }
}
