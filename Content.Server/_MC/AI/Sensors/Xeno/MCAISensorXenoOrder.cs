using Content.Shared._MC.AI;
using Content.Shared._MC.AI.Events;
using Content.Shared._MC.AI.Modules;
using Content.Shared._MC.Xeno.Abilities.Widow.Order;
using Content.Shared._RMC14.Xenonids.Rest;

namespace Content.Server._MC.AI.Sensors.Xeno;

public sealed partial class MCAISensorXenoOrder : MCAISensor<MCAISensorXenoOrder>
{
    public override TimeSpan UpdateInterval => TimeSpan.FromSeconds(0.1f);

    [DataField]
    public string Id = string.Empty;
}


public sealed class MCAISensorXenoOrderSystem : MCAISensorSystem<MCAISensorXenoOrder>
{
    protected override bool? OnSensorUpdate(Entity<MCAIAgentComponent> entity, ref MCAISensorUpdate<MCAISensorXenoOrder> args)
    {
        if (!TryComp<MCXenoOrderReceiverComponent>(entity, out var component))
            return false;

        return component.CurrentOrder == args.Sensor.Id;
    }
}
