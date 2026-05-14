using Content.Shared._MC.Engineering.Deploy.Components;

namespace Content.Shared._MC.Engineering.Deploy.Events;

[ByRefEvent]
public readonly record struct MCDeployChangedStateEvent(MCDeployState State, MCDeployState PreviousState);
