using Robust.Shared.Map;

namespace Content.Shared._MC.Engineering.Deploy.Events;

[ByRefEvent]
public struct MCDeployAttemptEvent(EntityCoordinates coordinates)
{
    public readonly EntityCoordinates Coordinates = coordinates;
    public bool Cancelled;
}
