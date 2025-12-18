using Content.Server._RMC14.Xenonids.Hive;
using Content.Server.Atmos.Components;
using Content.Shared._MC.Spreader;
using Content.Shared.Tag;
using Robust.Shared.Collections;
using Robust.Shared.Map.Components;
using Robust.Shared.Timing;

namespace Content.Server._MC.Spreader;

public sealed class MCEdgeSpreaderSystem : EntitySystem
{
    private static readonly Vector2i[] Directions = [Vector2i.Up, Vector2i.Right, Vector2i.Down, Vector2i.Left];

    [Dependency] private readonly IGameTiming _timing = null!;
    [Dependency] private readonly TagSystem _tag = null!;
    [Dependency] private readonly SharedMapSystem _map = null!;
    [Dependency] private readonly XenoHiveSystem _xenoHive = null!;

    private readonly List<SpawnDeferredEntry> _spawnDeferredEntries = [];

    private EntityQuery<AirtightComponent> _airtightQuery;

    public override void Initialize()
    {
        base.Initialize();

        _airtightQuery = GetEntityQuery<AirtightComponent>();
    }

    public override void Update(float frameTime)
    {
        var currentTime = _timing.CurTime;
        var query = EntityQueryEnumerator<MCEdgeSpreaderComponent, TransformComponent>();
        while (query.MoveNext(out var uid, out var component, out var xform))
        {
            if (component.NextUpdate > currentTime)
                continue;

            if (component.Range <= 0 || xform.GridUid is null)
            {
                RemCompDeferred<MCEdgeSpreaderComponent>(uid);
                continue;
            }

            GetFreeTiles(uid, xform, out var freeTiles);
            if (freeTiles.Count == 0)
            {
                RemCompDeferred<MCEdgeSpreaderComponent>(uid);
                continue;
            }

            _spawnDeferredEntries.Add(new SpawnDeferredEntry(uid, xform.GridUid.Value, component.Range - 1, currentTime + component.Delay, freeTiles));
            RemCompDeferred<MCEdgeSpreaderComponent>(uid);
        }

        foreach (var entry in _spawnDeferredEntries)
        {
            foreach (var tile in entry.FreeTiles)
            {
                var uid = SpawnSame(entry.Uid, entry.GridUid, tile);

                _xenoHive.SetSameHive(entry.Uid, uid);

                var spreader = EnsureComp<MCEdgeSpreaderComponent>(uid);

                spreader.Range = entry.Range;
                spreader.NextUpdate = entry.NextUpdate;

                DirtyFields(uid, spreader, null, nameof(MCEdgeSpreaderComponent.Range), nameof(MCEdgeSpreaderComponent.Delay));
            }
        }

        _spawnDeferredEntries.Clear();
    }

    private EntityUid SpawnSame(EntityUid uid, EntityUid gridUid, Vector2i tile)
    {
        var prototypeId = MetaData(uid).EntityPrototype?.ID;
        return Spawn(prototypeId, _map.GridTileToLocal(gridUid, Comp<MapGridComponent>(gridUid), tile));
    }

    private void GetFreeTiles(EntityUid uid, TransformComponent transformComponent, out ValueList<Vector2i> freeTiles)
    {
        freeTiles = [];

        if (!TryComp<MapGridComponent>(transformComponent.GridUid, out var grid))
            return;

        var tile = _map.TileIndicesFor(transformComponent.GridUid.Value, grid, transformComponent.Coordinates);
        foreach (var direction in  Directions)
        {
            var neighborPos = tile + direction;

            if (!_map.TryGetTileRef(transformComponent.GridUid.Value, grid, neighborPos, out var tileRef) || tileRef.Tile.IsEmpty)
                continue;

            if (SpaceBlocked((transformComponent.GridUid.Value, grid), neighborPos))
                continue;

            freeTiles.Add(neighborPos);
        }
    }

    private bool SpaceBlocked(Entity<MapGridComponent> grid, Vector2i pos)
    {
        var entities = _map.GetAnchoredEntitiesEnumerator(grid, grid, pos);
        while (entities.MoveNext(out var ent))
        {
            if (_tag.HasTag(ent.Value, $"MCSmoke"))
                return true;

            if (_airtightQuery.TryGetComponent(ent, out var airtight) && airtight.AirBlocked)
                return true;
        }

        return false;
    }

    private readonly record struct SpawnDeferredEntry(EntityUid Uid, EntityUid GridUid, int Range, TimeSpan NextUpdate, ValueList<Vector2i> FreeTiles);
}
