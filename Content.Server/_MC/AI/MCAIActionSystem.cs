using Content.Shared._MC.AI;
using Content.Shared._MC.AI.Events;
using Content.Shared._MC.AI.Modules;

namespace Content.Server._MC.AI;

public abstract class MCAIActionSystem<T> : EntitySystem where T : MCAIAction<T>
{
    [Dependency] protected readonly MCAISystem Ai = null!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MCAIAgentComponent, MCAIActionStartupEvent<T>>(OnActionStartup);
        SubscribeLocalEvent<MCAIAgentComponent, MCAIActionUpdateEvent<T>>(OnActionUpdateInternal);
        SubscribeLocalEvent<MCAIAgentComponent, MCAIActionShutdownEvent<T>>(OnActionShutdown);
        SubscribeLocalEvent<MCAIAgentComponent, MCAIActionCanExecuteEvent<T>>(OnCanExecuteInternal);
    }

    protected virtual void OnActionStartup(Entity<MCAIAgentComponent> entity, ref MCAIActionStartupEvent<T> args)
    {
    }

    protected virtual MCAIActionStatus OnActionUpdate(Entity<MCAIAgentComponent> entity, T action, float frameTime)
    {
        return MCAIActionStatus.Finished;
    }

    protected virtual void OnActionShutdown(Entity<MCAIAgentComponent> entity, ref MCAIActionShutdownEvent<T> args)
    {
    }

    protected virtual bool OnCanExecute(Entity<MCAIAgentComponent> entity, T action)
    {
        return true;
    }

    private void OnCanExecuteInternal(Entity<MCAIAgentComponent> ent, ref MCAIActionCanExecuteEvent<T> args)
    {
        args.Available = OnCanExecute(ent, args.Action);
    }

    private void OnActionUpdateInternal(Entity<MCAIAgentComponent> entity, ref MCAIActionUpdateEvent<T> args)
    {
        args.Status = OnActionUpdate(entity, args.Action, args.FrameTime);
    }
}
