using Content.Shared._MC.AI;
using Content.Shared._MC.AI.Components; // Подключаем наш новый компонент-маркер
using Content.Shared._MC.AI.Events;
using Content.Shared._MC.AI.Sensors;  // Подключаем наше новое определение сенсора
using Robust.Server.GameObjects;
using System.Linq;
using System.Numerics;

namespace Content.Server._MC.AI;

/// <summary>
/// Эта система обновляет сенсор MCAISensorDefendPoint.
/// Она ищет ближайшую сущность с компонентом MarineDefendPointComponent
/// и записывает ее в память ИИ.
/// </summary>
public sealed class MCAISensorDefendPointSystem : MCAISensorSystem<MCAISensorDefendPoint>
{
    [Dependency] private readonly TransformSystem _transform = default!;

    protected override bool? OnSensorUpdate(Entity<MCAIAgentComponent> entity, ref MCAISensorUpdate<MCAISensorDefendPoint> args)
    {
        EntityUid? closestTarget = null;
        var closestDistance = float.MaxValue;

        var agentPosition = _transform.GetWorldPosition(entity);

        // Ищем ВСЕ сущности с нашим компонентом-маркером на карте.
        // `EntityQuery<T>(true)` позволяет делать запрос ко всем сущностям, даже если они невидимы или выключены.
        var query = EntityQuery<MCMarineDefendPointComponent>(true);
        foreach (var (point, _) in query)
        {
            var pointPosition = _transform.GetWorldPosition(point);
            var distance = Vector2.Distance(agentPosition, pointPosition);

            if (distance >= closestDistance)
                continue;

            closestDistance = distance;
            closestTarget = point;
        }

        // Если мы не нашли ни одной точки на карте
        if (closestTarget is null)
        {
            // Очищаем память ИИ от старой цели, если она там была
            args.Memory.ContainerRemove(args.Sensor.OutputTargetKey);
            return false;
        }

        // Если нашли, записываем найденную цель в память ИИ
        args.Memory.ContainerSet(args.Sensor.OutputTargetKey, closestTarget.Value);
        return true;
    }
}
