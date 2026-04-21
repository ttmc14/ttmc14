using Content.Shared._MC.AI.Events;

namespace Content.Shared._MC.AI.Modules;

[ImplicitDataDefinitionForInheritors, Serializable]
public abstract partial class MCAISensorInternal
{
    [DataField, ViewVariables]
    public string ConditionKey = string.Empty;

    [ViewVariables]
    public virtual TimeSpan UpdateInterval => TimeSpan.Zero;

    [ViewVariables]
    public TimeSpan UpdateNext;

    public abstract void RaiseUpdate(IEntityManager manager, Entity<MCAIAgentComponent> agent);
}

public abstract partial class MCAISensor<T> : MCAISensorInternal where T : MCAISensor<T>
{
    public override void RaiseUpdate(IEntityManager manager, Entity<MCAIAgentComponent> agent)
    {
        if (this is not T self)
            return;

        var ev = new MCAISensorUpdate<T>(self, agent.Comp.Memory);
        manager.EventBus.RaiseLocalEvent(agent, ref ev);
    }
}

public abstract partial class MCAISensorNearestComponent<T> : MCAISensor<T> where T : MCAISensor<T>
{
    [DataField]
    public float VisionRadius = 10f;

    [DataField(required: true)]
    public string OutputTargetKey = string.Empty;
}
