using Content.Shared._MC.AI;
using Content.Shared._MC.AI.Events;
using Content.Shared._MC.AI.Modules;
using Content.Shared._RMC14.Xenonids.Rest;

namespace Content.Server._MC.AI.Sensors.Xeno;

public sealed partial class MCAISensorXenoResting : MCAISensor<MCAISensorXenoResting>
{
    public override TimeSpan UpdateInterval => TimeSpan.FromSeconds(0.1f);

    [DataField]
    public string TargetKey = string.Empty;
}


public sealed class MCAISensorCheckXenoRestingSystem : MCAISensorSystem<MCAISensorXenoResting>
{
    protected override bool? OnSensorUpdate(Entity<MCAIAgentComponent> entity, ref MCAISensorUpdate<MCAISensorXenoResting> args)
    {
        var uid = entity.Owner;
        if (entity.Comp.Memory.ContainerTryGet(args.Sensor.TargetKey, out EntityUid targetUid))
            uid = targetUid;

        return HasComp<XenoRestingComponent>(uid);
    }
}
