using Robust.Shared.Map;

namespace Content.Shared._MC.Shuttles.FTL;

public abstract class MCShuttleFTLSharedSystem : EntitySystem
{
    public abstract void FTLToCoordinates(
        EntityUid uid,
        EntityCoordinates coordinates,
        Angle angle,
        float? startupTime = null,
        float? hyperspaceTime = null,
        string? priorityTag = null
    );
}
