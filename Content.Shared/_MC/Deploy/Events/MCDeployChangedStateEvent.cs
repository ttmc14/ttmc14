namespace Content.Shared._MC.Deploy.Events;

[ByRefEvent]
public readonly record struct MCDeployChangedStateEvent(MCDeployState NewState, MCDeployState PreviousState);
