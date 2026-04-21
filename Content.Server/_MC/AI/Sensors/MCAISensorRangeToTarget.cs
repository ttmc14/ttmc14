using Content.Shared._MC.AI;
using Content.Shared._MC.AI.Events;
using Content.Shared._MC.AI.Modules;
using Robust.Server.GameObjects;

namespace Content.Server._MC.AI.Sensors;

public sealed partial class MCAISensorRangeToTarget : MCAISensor<MCAISensorRangeToTarget>
{
    public override TimeSpan UpdateInterval => TimeSpan.FromSeconds(0.05);

    [DataField(required: true)]
    public string TargetKey = string.Empty;

    [DataField]
    public float Range = 1.5f;
}

public sealed partial class MCAISensorRangeToTargetSystem : MCAISensorSystem<MCAISensorRangeToTarget>
{
    [Dependency] private readonly TransformSystem _transform = null!;

    protected override bool? OnSensorUpdate(Entity<MCAIAgentComponent> entity, ref MCAISensorUpdate<MCAISensorRangeToTarget> args)
    {
        if (!entity.Comp.Memory.ContainerTryGet<EntityUid>(args.Sensor.TargetKey, out var targetUid))
            return false;

        var delta = _transform.GetWorldPosition(entity) - _transform.GetWorldPosition(targetUid);
        return delta.LengthSquared() <= args.Sensor.Range * args.Sensor.Range;
    }
}

