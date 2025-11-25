using System.Numerics;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;

namespace Content.Shared._MC.Transform;

public sealed class MCSharedTransformSystem : EntitySystem
{
    [Dependency] private readonly IMapManager _mapManager = null!;

    [Dependency] private readonly SharedMapSystem _map = null!;
    [Dependency] private readonly SharedTransformSystem _transform = null!;

    private EntityQuery<TransformComponent> _xformQuery;
    private EntityQuery<MapGridComponent> _gridQuery;

    public override void Initialize()
    {
        base.Initialize();

        _xformQuery = GetEntityQuery<TransformComponent>();
        _gridQuery = GetEntityQuery<MapGridComponent>();
    }

    public void SetMapCoordinates(EntityUid entity, MapCoordinates coordinates, bool unanchor = true)
    {
        var xform = _xformQuery.GetComponent(entity);
        SetMapCoordinates((entity, xform), coordinates, unanchor);
    }

    public void SetMapCoordinates(Entity<TransformComponent> entity, MapCoordinates coordinates, bool unanchor = true)
    {
        var mapUid = _map.GetMap(coordinates.MapId);
        if (!_gridQuery.HasComponent(entity) && _mapManager.TryFindGridAt(mapUid, coordinates.Position, out var targetGrid, out _))
        {
            var invWorldMatrix = _transform.GetInvWorldMatrix(targetGrid);
            _transform.SetCoordinates((entity.Owner, entity.Comp, MetaData(entity.Owner)), new EntityCoordinates(targetGrid, Vector2.Transform(coordinates.Position, invWorldMatrix)), unanchor:  unanchor);
            return;
        }

        _transform.SetCoordinates((entity.Owner, entity.Comp, MetaData(entity.Owner)), new EntityCoordinates(mapUid, coordinates.Position), unanchor:  unanchor);
    }
}
