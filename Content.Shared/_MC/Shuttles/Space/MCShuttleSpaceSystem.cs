using Content.Shared._MC.Shuttles.Space.Components;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;

namespace Content.Shared._MC.Shuttles.Space;

public sealed partial class MCShuttleSpaceSystem : EntitySystem
{
    [Dependency] private readonly MetaDataSystem _metaData = null!;
    [Dependency] private readonly SharedMapSystem _map = null!;

    private MapId Create(string id, out Entity<MCShuttleSpaceComponent> entity)
    {
        var map = _map.CreateMap(out var mapId);
        var mapSpaceComponent = EnsureComp<MCShuttleSpaceComponent>(map);
        entity = (map, mapSpaceComponent);

        mapSpaceComponent.Id = id;
        Dirty(entity);

        _metaData.SetEntityName(map, $"mc-map-space {id}");

        return mapId;
    }

    private bool Get(string id, out MapId mapId, out Entity<MCShuttleSpaceComponent> entity)
    {
        var query = EntityQueryEnumerator<MCShuttleSpaceComponent, MapComponent>();
        while (query.MoveNext(out var uid, out var component, out var mapComponent))
        {
            mapId = mapComponent.MapId;
            entity = (uid, component);
            return true;
        }

        mapId = default;
        entity = default;
        return false;
    }
}
