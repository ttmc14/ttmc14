using Content.Server._MC.AI.Planner;
using Content.Shared._MC.AI;
using Content.Shared._MC.AI.Modules;

namespace Content.Server._MC.AI;

public sealed partial class MCAISystem
{
    private readonly List<MCAIActionInternal> _availableActions = new();
    private readonly List<MCGoalEntry> _availableGoals = new();

    private bool HasPlan(Entity<MCAIAgentComponent> entity)
    {
        return entity.Comp.Plan.Has;
    }

    private void Replan(Entity<MCAIAgentComponent> entity, bool force = false)
    {
        if (HasPlan(entity) && !force)
            return;

        RefreshAvailableActions(entity);
        RefreshAvailableGoals(entity);

        foreach (var (goal, goalIndex) in _availableGoals)
        {
            if (goalIndex == entity.Comp.Plan.CurrentActiveGoalId && !force)
                return;

            ActionCurrentShutdown(entity);

            entity.Comp.Plan.CurrentActionState = MCAIActionState.Starting;
            entity.Comp.Plan.Clear();

            if (!MCAIPlanner.Plan(entity.Comp.Memory, goal.DesiredState, _availableActions, entity.Comp.Plan.Current))
                continue;

            if (!entity.Comp.Plan.Has)
                continue;

            entity.Comp.Plan.CurrentActionId = 0;
            entity.Comp.Plan.CurrentActiveGoalId = goalIndex;
            return;
        }

        ClearPlan(entity);
    }

    private void RefreshAvailableActions(Entity<MCAIAgentComponent> entity)
    {
        _availableActions.Clear();
        foreach (var action in entity.Comp.Actions)
        {
            if (ActionExecutable(entity, action))
                _availableActions.Add(action);
        }
    }

    private void RefreshAvailableGoals(Entity<MCAIAgentComponent> entity)
    {
        _availableGoals.Clear();

        for (var i = 0; i < entity.Comp.Goals.Count; i++)
        {
            var entry = new MCGoalEntry(entity.Comp.Goals[i], i);

            if (!entity.Comp.Memory.StateCheckPreconditions(entry.Instance.Preconditions))
                continue;

            if (entity.Comp.Memory.StateCheckPreconditions(entry.Instance.DesiredState))
                continue;

            _availableGoals.Add(entry);
        }

        _availableGoals.Sort((a, b) => b.Instance.Priority.CompareTo(a.Instance.Priority));
    }

    private void ClearPlan(Entity<MCAIAgentComponent> entity)
    {
        ActionCurrentShutdown(entity);

        entity.Comp.Plan.Clear();
    }

    private readonly record struct MCGoalEntry(MCGoal Instance, int Index);
}
