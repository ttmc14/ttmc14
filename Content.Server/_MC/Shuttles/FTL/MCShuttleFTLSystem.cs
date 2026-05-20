using Content.Server.Shuttles.Components;
using Content.Server.Shuttles.Systems;
using Content.Shared._MC.Shuttles.FTL;
using Robust.Shared.Map;

namespace Content.Server._MC.Shuttles.FTL;

public sealed class MCShuttleFTLSystem : MCShuttleFTLSharedSystem
{
    [Dependency] private readonly ShuttleSystem _shuttle = null!;

    private EntityQuery<ShuttleComponent> _shuttleQuery;

    public override void Initialize()
    {
        _shuttleQuery = GetEntityQuery<ShuttleComponent>();
    }

    public override void FTLToCoordinates(EntityUid uid,
        EntityCoordinates coordinates,
        Angle angle,
        float? startupTime = null,
        float? hyperspaceTime = null,
        string? priorityTag = null)
    {
        if (!_shuttleQuery.TryComp(uid, out var component))
            return;

        _shuttle.FTLToCoordinates(uid, component, coordinates, angle, startupTime, hyperspaceTime, priorityTag);
    }
}
