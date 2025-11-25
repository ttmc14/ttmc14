using Content.Shared._MC.Operation.Events;
using Robust.Shared.GameStates;

namespace Content.Shared._MC.Operation;

public sealed class MCOperationSystem : MCEntitySystemSingleton<MCOperationSystemSingletonComponent>
{
    public bool Started => Inst.Comp.Started;

    public void Start()
    {
        if (Inst.Comp.Started)
            return;

        Inst.Comp.Started = true;
        Dirty(Inst);

        var ev = new MCOperationStartEvent();
        RaiseLocalEvent(ev);
    }
}

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MCOperationSystemSingletonComponent : Component
{
    [DataField, AutoNetworkedField]
    public bool Started;
}
