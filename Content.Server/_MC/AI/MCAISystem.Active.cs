using Content.Shared._MC.AI;
using Content.Shared.NPC;
using JetBrains.Annotations;

namespace Content.Server._MC.AI;

public sealed partial class MCAISystem
{
    [PublicAPI]
    public void AgentWake(Entity<MCAIAgentComponent?> entity)
    {
        if (!Resolve(entity, ref entity.Comp, false))
            return;

        EnsureComp<ActiveNPCComponent>(entity);
        AgentActiveListAdd(entity);
    }

    [PublicAPI]
    public void AgentSleep(Entity<MCAIAgentComponent?> entity)
    {
        if (!Resolve(entity, ref entity.Comp, false))
            return;

        RemComp<ActiveNPCComponent>(entity);
        AgentActiveListRemove(entity);
    }

    private void AgentActiveListAdd(EntityUid uid)
    {
        if (!_activeAgents.Contains(uid) && !_toAdd.Contains(uid))
            _toAdd.Add(uid);
    }

    private void AgentActiveListRemove(EntityUid uid)
    {
        if (_activeAgents.Contains(uid) && !_toRemove.Contains(uid))
            _toRemove.Add(uid);
    }
}
