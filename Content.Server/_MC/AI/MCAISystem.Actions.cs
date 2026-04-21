using Content.Shared._MC.AI;
using Content.Shared._MC.AI.Modules;

namespace Content.Server._MC.AI;

public sealed partial class MCAISystem
{
private void ActionCurrentExecute(Entity<MCAIAgentComponent> entity, float frameTime)
    {
        var action = entity.Comp.Plan.CurrentAction;
        if (entity.Comp.Plan.CurrentActionState == MCAIActionState.Starting)
        {
            ActionStartup(entity, action);
            entity.Comp.Plan.CurrentActionState = MCAIActionState.Running;
        }

        var status = ActionUpdate(entity, action, frameTime);
        switch (status)
        {
            case MCAIActionStatus.Finished:
                ActionShutdown(entity, action);

                entity.Comp.Plan.Next();

                if (entity.Comp.Plan.Complete)
                    entity.Comp.Plan.Clear();

                break;

            case MCAIActionStatus.Failed:
                ActionShutdown(entity, action);

                entity.Comp.Plan.Clear();
                entity.Comp.Plan.ForceReplanTime();
                break;

            case MCAIActionStatus.Running:
                break;

            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void ActionCurrentShutdown(Entity<MCAIAgentComponent> entity)
    {
        if (!entity.Comp.Plan.Has)
            return;

        if (entity.Comp.Plan.CurrentActionState == MCAIActionState.Starting)
            return;

        if (entity.Comp.Plan.CurrentActionId >= entity.Comp.Plan.Size)
            return;

        ActionShutdown(entity, entity.Comp.Plan.CurrentAction);
    }

    private MCAIActionStatus ActionUpdate(Entity<MCAIAgentComponent> entity, MCAIActionInternal action, float frameTime)
    {
        return action.RaiseUpdate(EntityManager, entity, frameTime);
    }

    private void ActionStartup(Entity<MCAIAgentComponent> entity, MCAIActionInternal action)
    {
        action.RaiseStartup(EntityManager, entity);
    }

    private void ActionShutdown(Entity<MCAIAgentComponent> entity, MCAIActionInternal action)
    {
        action.RaiseShutdown(EntityManager, entity);
    }

    private bool ActionExecutable(Entity<MCAIAgentComponent> entity, MCAIActionInternal action)
    {
        return action.RaiseExecutable(EntityManager, entity);
    }
}
