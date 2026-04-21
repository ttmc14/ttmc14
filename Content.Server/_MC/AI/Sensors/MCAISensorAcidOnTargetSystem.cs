using Content.Shared._MC.AI;
using Content.Shared._MC.AI.Events;
using Content.Shared._MC.AI.Modules;
using Content.Shared._RMC14.Xenonids.Acid;

namespace Content.Server._MC.AI.Sensors;

public sealed partial class MCAISensorAcidOnTarget : MCAISensor<MCAISensorAcidOnTarget>
{
    public override TimeSpan UpdateInterval => TimeSpan.FromSeconds(0.1f);

    [DataField(required: true)]
    public string TargetKey = string.Empty;
}

public sealed class MCAISensorAcidOnTargetSystem : MCAISensorSystem<MCAISensorAcidOnTarget>
{
    [Dependency] private readonly SharedXenoAcidSystem _xenoAcid = null!;

    protected override bool? OnSensorUpdate(Entity<MCAIAgentComponent> entity, ref MCAISensorUpdate<MCAISensorAcidOnTarget> args)
    {
        return entity.Comp.Memory.ContainerTryGet<EntityUid>(args.Sensor.TargetKey, out var targetUid) && _xenoAcid.IsMelted(targetUid);
    }
}
