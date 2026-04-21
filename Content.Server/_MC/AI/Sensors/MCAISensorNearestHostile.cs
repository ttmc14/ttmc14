using System.Numerics;
using Content.Shared._MC.AI;
using Content.Shared._MC.AI.Events;
using Content.Shared._MC.AI.Modules;
using Content.Shared.Examine;
using Content.Shared.Mobs.Systems;
using Content.Shared.NPC.Systems;

namespace Content.Server._MC.AI.Sensors;

public sealed partial class MCAISensorNearestHostile : MCAISensor<MCAISensorNearestHostile>
{
    public override TimeSpan UpdateInterval => TimeSpan.FromSeconds(0.075f);

    [DataField]
    public float VisionRadius = 10f;

    [DataField(required: true)]
    public string OutputTargetKey = string.Empty;
}

public sealed partial class MCAISensorNearestHostileSystem : MCAISensorSystem<MCAISensorNearestHostile>
{
    [Dependency] private readonly SharedTransformSystem _transform = null!;
    [Dependency] private readonly ExamineSystemShared _examine = null!;
    [Dependency] private readonly NpcFactionSystem _faction = null!;
    [Dependency] private readonly MobStateSystem _mobState = null!;

    private EntityQuery<TransformComponent> _transformQuery;

    public override void Initialize()
    {
        base.Initialize();

        _transformQuery = GetEntityQuery<TransformComponent>();
    }

    protected override bool? OnSensorUpdate(Entity<MCAIAgentComponent> entity, ref MCAISensorUpdate<MCAISensorNearestHostile> args)
    {
        var position = _transform.GetWorldPosition(entity);
        var hostiles = _faction.GetNearbyHostiles((entity.Owner, null, null), args.Sensor.VisionRadius);

        EntityUid? closestTarget = null;
        var closestDistance = float.MaxValue;

        foreach (var targetUid in hostiles)
        {
            if (!_transformQuery.TryGetComponent(targetUid, out var targetXform))
                continue;

            var targetWorldPos = _transform.GetWorldPosition(targetXform);
            var distance = Vector2.Distance(position, targetWorldPos);

            if (distance >= closestDistance)
                continue;

            // if (!_examine.InRangeUnOccluded(entity.Owner, targetUid, args.Sensor.VisionRadius + 0.5f))
            //    continue;

            if (_mobState.IsDead(targetUid))
                continue;

            closestDistance = distance;
            closestTarget = targetUid;
        }

        if (closestTarget is null)
            return false;

        entity.Comp.Memory.ContainerSet(args.Sensor.OutputTargetKey, closestTarget.Value);
        return true;
    }
}
