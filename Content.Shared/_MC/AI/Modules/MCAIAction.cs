using Content.Shared._MC.AI.Events;

namespace Content.Shared._MC.AI.Modules;

[ImplicitDataDefinitionForInheritors, Serializable]
public abstract partial class MCAIActionInternal
{
    [DataField]
    public Dictionary<string, bool> Preconditions = new();

    [DataField]
    public Dictionary<string, bool> Effects = new();

    [DataField]
    public float Cost = 1f;

    public abstract MCAIActionStatus RaiseUpdate(IEntityManager manager, Entity<MCAIAgentComponent> agent, float frameTime);
    public abstract void RaiseStartup(IEntityManager manager, Entity<MCAIAgentComponent> agent);
    public abstract void RaiseShutdown(IEntityManager manager, Entity<MCAIAgentComponent> agent);
    public abstract bool RaiseExecutable(IEntityManager manager, Entity<MCAIAgentComponent> agent);
}

public abstract partial class MCAIAction<T> : MCAIActionInternal where T : MCAIAction<T>
{
    public override MCAIActionStatus RaiseUpdate(IEntityManager manager, Entity<MCAIAgentComponent> agent, float frameTime)
    {
        if (this is not T self)
            return default;

        var ev = new MCAIActionUpdateEvent<T>(self, frameTime);
        manager.EventBus.RaiseLocalEvent(agent, ref ev);
        return ev.Status;
    }

    public override void RaiseStartup(IEntityManager manager, Entity<MCAIAgentComponent> agent)
    {
        if (this is not T self)
            return;

        var ev = new MCAIActionStartupEvent<T>(self);
        manager.EventBus.RaiseLocalEvent(agent, ref ev);
    }

    public override void RaiseShutdown(IEntityManager manager, Entity<MCAIAgentComponent> agent)
    {
        if (this is not T self)
            return;

        var ev = new MCAIActionShutdownEvent<T>(self);
        manager.EventBus.RaiseLocalEvent(agent, ref ev);
    }

    public override bool RaiseExecutable(IEntityManager manager, Entity<MCAIAgentComponent> agent)
    {
        if (this is not T self)
            return  false;

        var ev = new MCAIActionCanExecuteEvent<T>(self);
        manager.EventBus.RaiseLocalEvent(agent, ref ev);
        return ev.Available;
    }
}
