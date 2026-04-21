using Content.Shared._MC.AI;
using Content.Shared._MC.AI.Events;
using Content.Shared._MC.AI.Modules;
using Content.Shared._MC.Xeno.Heal;
using Content.Shared.Damage;

namespace Content.Server._MC.AI.Sensors;

public sealed partial class MCAISensorCheckHealthLevel : MCAISensor<MCAISensorCheckHealthLevel>
{
    [DataField]
    public float Threshold = 0.5f;
}

public sealed partial class MCAISensorCheckHealthLevelSystem : MCAISensorSystem<MCAISensorCheckHealthLevel>
{
    [Dependency] private readonly MCXenoHealSystem _mcXenoHeal = null!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MCAIAgentComponent, DamageChangedEvent>(OnDamageChanged);
    }

    private void OnDamageChanged(Entity<MCAIAgentComponent> entity, ref DamageChangedEvent args)
    {
        var fraction = GetFraction(entity);
        foreach (var sensor in entity.Comp.Sensors)
        {
            if (sensor is not MCAISensorCheckHealthLevel healthSensor)
                continue;

            entity.Comp.Memory.StateSet(healthSensor.ConditionKey, fraction < healthSensor.Threshold);
        }
    }

    protected override bool? OnSensorUpdate(Entity<MCAIAgentComponent> entity, ref MCAISensorUpdate<MCAISensorCheckHealthLevel> args)
    {
        var fraction = GetFraction(entity);
        return fraction < args.Sensor.Threshold;
    }

    private float GetFraction(EntityUid uid)
    {
        var health = _mcXenoHeal.GetHealth(uid);
        var maxHealth = _mcXenoHeal.GetHealthAlive(uid);
        return health / maxHealth;
    }
}
