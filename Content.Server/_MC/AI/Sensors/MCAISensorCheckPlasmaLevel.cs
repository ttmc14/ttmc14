using Content.Shared._MC.AI;
using Content.Shared._MC.AI.Events;
using Content.Shared._MC.AI.Modules;
using Content.Shared._MC.Xeno.Heal;
using Content.Shared._MC.Xeno.Plasma.Systems;
using Content.Shared.Damage;

namespace Content.Server._MC.AI.Sensors;

public sealed partial class MCAISensorCheckPlasmaLevel : MCAISensor<MCAISensorCheckPlasmaLevel>
{
    public override TimeSpan UpdateInterval => TimeSpan.FromSeconds(0.05f);

    [DataField]
    public float Threshold = 0.5f;

    [DataField]
    public Directions Direction = Directions.Less;

    public enum Directions : byte
    {
        Less,
        Greater,
    }
}

public sealed class MCAISensorCheckPlasmaLevelSystem : MCAISensorSystem<MCAISensorCheckPlasmaLevel>
{
    [Dependency] private readonly MCXenoPlasmaSystem _mcXenoPlasma = null!;

    protected override bool? OnSensorUpdate(Entity<MCAIAgentComponent> entity, ref MCAISensorUpdate<MCAISensorCheckPlasmaLevel> args)
    {
        return args.Sensor.Direction switch
        {
            MCAISensorCheckPlasmaLevel.Directions.Less => _mcXenoPlasma.GetPlasmaNormalized(entity) < args.Sensor.Threshold,
            MCAISensorCheckPlasmaLevel.Directions.Greater => _mcXenoPlasma.GetPlasmaNormalized(entity) > args.Sensor.Threshold,
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}
