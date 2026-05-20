using Content.Shared._MC.Shuttles.FTL;
using Robust.Shared.Map;

namespace Content.Client._MC.Shuttles.FTL;

public sealed class MCShuttleFTLSystem : MCShuttleFTLSharedSystem
{
    public override void FTLToCoordinates(EntityUid uid,
        EntityCoordinates coordinates,
        Angle angle,
        float? startupTime = null,
        float? hyperspaceTime = null,
        string? priorityTag = null)
    {
    }
}
