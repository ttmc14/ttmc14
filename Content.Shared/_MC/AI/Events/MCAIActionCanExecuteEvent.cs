using Content.Shared._MC.AI.Modules;

namespace Content.Shared._MC.AI.Events;

[ByRefEvent]
public struct MCAIActionCanExecuteEvent<T>(T action) where T : MCAIAction<T>
{
    public readonly T Action = action;
    public bool Available = true;
}
