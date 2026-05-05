using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Map.Enumerators;

namespace Content.Shared._MC.Map;

public sealed class MCAnchoredRadiusSystem : EntitySystem
{
    [Dependency] private readonly SharedMapSystem _map = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    private EntityQuery<MapGridComponent> _gridQuery;

    public override void Initialize()
    {
        _gridQuery = GetEntityQuery<MapGridComponent>();
    }

    public void GetAnchoredInRadius(
        HashSet<EntityUid> list,
        EntityCoordinates coords,
        int radius)
    {
        var enumerator = GetAnchoredInRadius(coords, radius);
        while (enumerator.MoveNext(out var uid))
        {
            list.Add(uid);
        }
    }

    public MCAnchoredRadiusEnumerator GetAnchoredInRadius(
        EntityCoordinates coords,
        int radius)
    {
        if (_transform.GetGrid(coords) is not { } gridUid ||
            !_gridQuery.TryComp(gridUid, out var grid))
        {
            return MCAnchoredRadiusEnumerator.Empty;
        }

        var center = _map.TileIndicesFor(gridUid, grid, coords);

        return new MCAnchoredRadiusEnumerator(
            _map,
            gridUid,
            grid,
            center,
            radius);
    }
}

public struct MCAnchoredRadiusEnumerator
{
    private readonly SharedMapSystem _map;
    private readonly EntityUid _gridUid;
    private readonly MapGridComponent _grid;
    private readonly Vector2i _center;
    private readonly int _radius;

    private int _x;
    private int _y;

    private AnchoredEntitiesEnumerator _tileEnumerator;
    private bool _hasEnumerator;

    public static MCAnchoredRadiusEnumerator Empty => new();

    public MCAnchoredRadiusEnumerator(
        SharedMapSystem map,
        EntityUid gridUid,
        MapGridComponent grid,
        Vector2i center,
        int radius)
    {
        _map = map;
        _gridUid = gridUid;
        _grid = grid;
        _center = center;
        _radius = radius;

        _x = -radius;
        _y = -radius;

        _tileEnumerator = default;
        _hasEnumerator = false;
    }

    public bool MoveNext(out EntityUid uid)
    {
        uid = default;

        while (true)
        {
            if (_hasEnumerator)
            {
                if (_tileEnumerator.MoveNext(out var found))
                {
                    uid = found.Value;
                    return true;
                }

                _tileEnumerator.Dispose();
                _hasEnumerator = false;
            }

            if (_y > _radius)
                return false;

            if (_x * _x + _y * _y <= _radius * _radius)
            {
                var indices = new Vector2i(
                    _center.X + _x,
                    _center.Y + _y);

                _tileEnumerator = _map.GetAnchoredEntitiesEnumerator(
                    _gridUid,
                    _grid,
                    indices);

                _hasEnumerator = true;
            }

            _x++;

            if (_x <= _radius)
                continue;

            _x = -_radius;
            _y++;
        }
    }
}
