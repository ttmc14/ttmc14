using Content.Shared._MC.Shuttles.TargetPoint.Components;
using Robust.Shared.Map;

namespace Content.Shared._MC.Shuttles.TargetPoint;

public sealed class MCShuttleTargetPointSystem : EntitySystem
{
    [Dependency] private readonly SharedTransformSystem _transform = null!;

    public bool TryGetPointCoordinates(string id, out MapCoordinates coordinates)
    {
        // TODO: cache optimization

        var query = EntityQueryEnumerator<MCShuttleTargetPointComponent>();
        while (query.MoveNext(out var uid, out var component))
        {
            if (component.Id != id)
                continue;

            coordinates = _transform.GetMapCoordinates(uid);
            return true;
        }

        coordinates = default;
        return false;
    }
}
