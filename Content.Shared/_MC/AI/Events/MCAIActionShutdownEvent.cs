using Content.Shared._MC.AI.Modules;

namespace Content.Shared._MC.AI.Events;

[ByRefEvent]
public record struct MCAIActionShutdownEvent<T>(T Action) where T : MCAIAction<T>;
