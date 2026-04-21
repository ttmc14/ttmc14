using Content.Shared._MC.AI.Modules;

namespace Content.Shared._MC.AI.Events;

[ByRefEvent]
public record struct MCAIActionStartupEvent<T>(T Action) where T : MCAIAction<T>;
