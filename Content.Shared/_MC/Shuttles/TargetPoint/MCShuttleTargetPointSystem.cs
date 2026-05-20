using Content.Shared._MC.Shuttles.TargetPoint.Components;
using Robust.Shared.Map;

namespace Content.Shared._MC.Shuttles.TargetPoint;

public sealed class MCShuttleTargetPointSystem : EntitySystem
{
    [Dependency] private readonly SharedTransformSystem _transform = null!;

    public bool TryGetPointCoordinates(string id, out MapCoordinates coordinates, out EntityUid entity)
    {
        var query = EntityQueryEnumerator<MCShuttleTargetPointComponent>();
        while (query.MoveNext(out var uid, out var component))
        {
            if (component.Id != id)
                continue;

            entity = uid;

            coordinates = _transform.GetMapCoordinates(uid);
            coordinates = new MapCoordinates(coordinates.Position + component.Offset, coordinates.MapId);

            return true;
        }

        entity = default;
        coordinates = default;
        return false;
    }

    public bool TryGetPointCoordinates(string id, out EntityCoordinates coordinates)
    {
        // TODO: cache optimization

        var query = EntityQueryEnumerator<MCShuttleTargetPointComponent>();
        while (query.MoveNext(out var uid, out var component))
        {
            if (component.Id != id)
                continue;

            coordinates = Transform(uid).Coordinates;
            coordinates = coordinates.WithPosition(coordinates.Position + component.Offset);
            return true;
        }

        coordinates = default;
        return false;
    }
}
