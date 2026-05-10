using Content.Shared._MC.AI;
using Content.Shared._MC.AI.Events;
using Content.Shared._MC.AI.Modules;
using Content.Shared._MC.Xeno.Abilities.Widow.Summon;

namespace Content.Server._MC.AI.Sensors.Xeno;

public sealed partial class MCAISensorXenoOwner : MCAISensor<MCAISensorXenoOwner>
{
    public override TimeSpan UpdateInterval => TimeSpan.FromSeconds(0.1f);

    [DataField(required: true)]
    public string OutputTargetKey = string.Empty;
}

public sealed class MCAISensorXenoOwnerSystem : MCAISensorSystem<MCAISensorXenoOwner>
{
    protected override bool? OnSensorUpdate(Entity<MCAIAgentComponent> entity, ref MCAISensorUpdate<MCAISensorXenoOwner> args)
    {
        if (!TryComp<MCXenoSummonedComponent>(entity, out var component))
            return false;

        entity.Comp.Memory.ContainerSet(args.Sensor.OutputTargetKey, component.OwnerUid);
        return true;
    }
}
