using Content.Shared._MC.AI;
using Content.Shared._MC.AI.Events;
using Content.Shared._MC.AI.Modules;
using Content.Shared.Mobs.Systems;

namespace Content.Server._MC.AI.Sensors;

public sealed partial class MCAISensorTargetDead : MCAISensor<MCAISensorTargetDead>
{
    public override TimeSpan UpdateInterval => TimeSpan.FromSeconds(0.05f);

    [DataField(required: true)]
    public string TargetKey = string.Empty;
}

public sealed class MCAISensorTargetDeadSystem : MCAISensorSystem<MCAISensorTargetDead>
{
    [Dependency] private readonly MobStateSystem _mobState = null!;

    protected override bool? OnSensorUpdate(Entity<MCAIAgentComponent> entity, ref MCAISensorUpdate<MCAISensorTargetDead> args)
    {
        if (!entity.Comp.Memory.ContainerTryGet<EntityUid>(args.Sensor.TargetKey, out var target))
            return false;

        if (!Exists(target) || TerminatingOrDeleted(target))
            return true;

        return _mobState.IsDead(target);
    }
}
