using Content.Shared._MC.AI.Modules;

namespace Content.Shared._MC.AI.Events;

[ByRefEvent]
public record struct MCAIActionUpdateEvent<T>(T Action, float FrameTime) where T : MCAIAction<T>
{
    public MCAIActionStatus Status = MCAIActionStatus.Failed;
}
