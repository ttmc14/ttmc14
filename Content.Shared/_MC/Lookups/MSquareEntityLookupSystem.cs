using System.Numerics;
using Robust.Shared.Map;

namespace Content.Shared._MC.Lookups;

public sealed class MSquareEntityLookupSystem : EntitySystem
{
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly EntityLookupSystem _entityLookup = default!;

    public IEnumerable<EntityUid> GetEntitiesInDirectionalArea(
        EntityUid originUid,
        Vector2 cardinalDirection,
        Vector2 nearSize,
        Vector2 farSize,
        float farOffset = 2f)
    {
        var origin = _transform.GetMapCoordinates(originUid);

        var halfNear = nearSize / 2f;
        var nearBox = new Box2(origin.Position - halfNear, origin.Position + halfNear);

        foreach (var uid in _entityLookup.GetEntitiesIntersecting(origin.MapId, nearBox))
        {
            yield return uid;
        }

        var farCenter = origin.Position + cardinalDirection * farOffset;
        var halfFar = farSize / 2f;
        var farBox = new Box2(farCenter - halfFar, farCenter + halfFar);

        foreach (var uid in _entityLookup.GetEntitiesIntersecting(origin.MapId, farBox))
        {
            yield return uid;
        }
    }

    public IEnumerable<EntityUid> GetEntitiesInBox(MapId mapId, Box2 box)
    {
        return _entityLookup.GetEntitiesIntersecting(mapId, box);
    }
}
