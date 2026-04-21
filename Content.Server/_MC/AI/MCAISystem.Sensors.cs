using Content.Shared._MC.AI;
using Content.Shared._MC.AI.Modules;

namespace Content.Server._MC.AI;

public sealed partial class MCAISystem
{
    private void SensorUpdate(Entity<MCAIAgentComponent> entity, MCAISensorInternal sensor)
    {
        // Haha we have no reflection so we go FUCK
        sensor.RaiseUpdate(EntityManager, entity);
    }
}
