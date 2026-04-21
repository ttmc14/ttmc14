using Content.Shared._MC.AI;
using Content.Shared.NPC;

namespace Content.Server._MC.AI;

public sealed partial class MCAISystem
{
    public void AgentWake(Entity<MCAIAgentComponent?> entity)
    {
        if (!Resolve(entity, ref entity.Comp, false))
            return;

        EnsureComp<ActiveNPCComponent>(entity);
        _activeAgents.Add(new Entity<MCAIAgentComponent>(entity, entity.Comp));
    }

    public void AgentSleep(Entity<MCAIAgentComponent?> entity)
    {
        if (!Resolve(entity, ref entity.Comp, false))
            return;

        RemComp<ActiveNPCComponent>(entity);
        _activeAgents.Remove(new Entity<MCAIAgentComponent>(entity, entity.Comp));
    }
}
