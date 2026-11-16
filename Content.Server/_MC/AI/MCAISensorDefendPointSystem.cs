using Content.Shared._MC.AI;
using Content.Shared._MC.Rules.DefendPoint;
using Content.Shared._MC.AI.Events;
using Content.Shared._MC.AI.Sensors;
using Robust.Server.GameObjects;
using System.Linq;
using System.Numerics;
using System.Collections.Generic;

namespace Content.Server._MC.AI;

public sealed class MCAISensorDefendPointSystem : MCAISensorSystem<MCAISensorDefendPoint>
{
    [Dependency] private readonly TransformSystem _transform = default!;

    protected override bool? OnSensorUpdate(Entity<MCAIAgentComponent> entity, ref MCAISensorUpdate<MCAISensorDefendPoint> args)
    {
        EntityUid? closestTarget = null;
        var closestDistance = float.MaxValue;

        var agentPosition = _transform.GetWorldPosition(entity);

        // Исправленный цикл
        var query = EntityQueryEnumerator<MCDefendPointComponent>();
        while (query.MoveNext(out var point, out _))
        {
            var pointPosition = _transform.GetWorldPosition(point);
            var distance = Vector2.Distance(agentPosition, pointPosition);

            if (distance >= closestDistance)
                continue;

            closestDistance = distance;
            closestTarget = point;
        }

        if (closestTarget is null)
        {
            // Исправленная передача аргумента
            args.Memory.ContainerRemove(new List<string> { args.Sensor.OutputTargetKey });
            return false;
        }

        args.Memory.ContainerSet(args.Sensor.OutputTargetKey, closestTarget.Value);
        return true;
    }
}
