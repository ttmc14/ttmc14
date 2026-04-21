using Content.Shared._MC.AI;
using Content.Shared.Mobs;
using Robust.Shared.Player;

namespace Content.Server._MC.AI;

public sealed partial class MCAISystem
{
    private void OnAgentMapInit(Entity<MCAIAgentComponent> entity, ref MapInitEvent args)
    {
        foreach (var action in entity.Comp.Actions)
        {
            entity.Comp.Memory.StateWriteKeys(action.Preconditions);
            entity.Comp.Memory.StateWriteKeys(action.Effects);
        }

        foreach (var goal in entity.Comp.Goals)
        {
            entity.Comp.Memory.StateWriteKeys(goal.DesiredState);
            entity.Comp.Memory.StateWriteKeys(goal.Preconditions);
        }

        UpdateSensors(entity, force: true);
        AgentWake((entity, entity.Comp));
    }

    private void OnAgentShutdown(Entity<MCAIAgentComponent> entity, ref ComponentShutdown args)
    {
        ClearPlan(entity);
        AgentSleep((entity, entity.Comp));
    }

    private void OnAgentStateChanged(Entity<MCAIAgentComponent> entity, ref MobStateChangedEvent args)
    {
        if (args.NewMobState == MobState.Dead)
        {
            AgentSleep((entity, entity.Comp));
            return;
        }

        AgentWake((entity, entity.Comp));
    }

    private void OnAgentPlayerAttach(Entity<MCAIAgentComponent> entity, ref PlayerAttachedEvent args)
    {
        AgentSleep((entity, entity.Comp));
    }

    private void OnAgentPlayerDetach(Entity<MCAIAgentComponent> entity, ref PlayerDetachedEvent args)
    {
        AgentWake((entity, entity.Comp));
    }
}
