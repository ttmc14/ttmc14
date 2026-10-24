using System.Linq;
using Content.Server._MC.Xeno.Hive;
using Content.Server._RMC14.MapInsert;
using Content.Shared._MC.Map;
using Content.Shared._RMC14.Rules;
using Content.Shared._RMC14.TacticalMap;
using Content.Shared._RMC14.Xenonids;
using Robust.Server.GameObjects;
using Robust.Shared.Configuration;
using Robust.Shared.EntitySerialization.Systems;
using Robust.Shared.Map.Components;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Server._MC.Xeno.Spawn;

public sealed partial class MCXenoSpawnSystem : EntitySystem
{
    [Dependency] private readonly IConfigurationManager _config = null!;
    [Dependency] private readonly IRobustRandom _random = null!;

    [Dependency] private readonly MapSystem _map = null!;
    [Dependency] private readonly MapInsertSystem _mapInsert = null!;
    [Dependency] private readonly MapLoaderSystem _mapLoader = null!;

    [Dependency] private readonly RMCPlanetSystem _rmcPlanet = null!;
    [Dependency] private readonly MCXenoHiveSystem _mcXenoHive = null!;

    [ViewVariables]
    private readonly Queue<EntProtoId<RMCPlanetMapPrototypeComponent>> _lastPlanetMaps = new();

    [ViewVariables]
    private RMCPlanet? _selectedPlanetMap;

    public override void Initialize()
    {
        InitializeVote();
    }

    private void SetFriendlyHives(EntityUid? hiveUid)
    {
        if (!Exists(hiveUid))
            return;

        var query = EntityQueryEnumerator<XenoFriendlyComponent>();
        while (query.MoveNext(out var uid, out _))
        {
            _mcXenoHive.SetHive(uid, hiveUid);
        }
    }

    private bool TryLoadAndInitializeMap(RMCPlanet planet, out Entity<MapComponent> entity, out EntityUid gridUid)
    {
        entity = default;
        gridUid = default;

        if (!_mapLoader.TryLoadMap(planet.Comp.Map, out var mapNullable, out var grids))
        {
            Log.Error("MapLoader failed to load the planet map prototype.");
            return false;
        }

        entity = mapNullable.Value;

        switch (grids.Count)
        {
            case 0:
                Log.Error("Loaded map contains no grids.");
                return false;
            case > 1:
                Log.Warning($"Multiple grids found for planet {planet.Proto.Name}. Using the first one.");
                break;
        }

        gridUid = grids.First();

        EnsureComp<RMCPlanetComponent>(entity);
        EnsureComp<TacticalMapComponent>(entity);

        _map.InitializeMap((entity, entity));

        return true;
    }

    private void ProcessMapInsertions()
    {
        var query = EntityQueryEnumerator<MapInsertComponent>();
        while (query.MoveNext(out var uid, out var component))
        {
            _mapInsert.ProcessMapInsert((uid, component));
        }
    }

    private void RaisePlanetSpawnEvent(Entity<MapComponent> map, RMCPlanet planet)
    {
        var ev = new MCPlanetMapSpawnEvent(map, planet.Proto);
        RaiseLocalEvent(ref ev);
    }

    private void UpdatePlanetHistory(EntProtoId<RMCPlanetMapPrototypeComponent> planetId)
    {
        _lastPlanetMaps.Enqueue(planetId);
        while (_lastPlanetMaps.Count > _voteExcludeLast)
        {
            _lastPlanetMaps.Dequeue();
        }
    }
}
