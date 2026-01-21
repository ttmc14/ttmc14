using Content.Server._CE.ZLevels.Core;
using Content.Shared._MC.Map;
using Robust.Server.GameObjects;
using Robust.Shared.EntitySerialization.Systems;
using Robust.Shared.Map.Components;

namespace Content.Server._MC.Map;

public sealed class MCPlanetMapSystem : EntitySystem
{
    [Dependency] private readonly MapLoaderSystem _mapLoader = null!;
    [Dependency] private readonly CEZLevelsSystem _zLevel = null!;
    [Dependency] private readonly MetaDataSystem _meta = null!;
    [Dependency] private readonly MapSystem _map = null!;
    [Dependency] private readonly IComponentFactory _componentFactory = null!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MCPlanetMapSpawnEvent>(OnSpawn);
    }

    private void OnSpawn(ref MCPlanetMapSpawnEvent ev)
    {
        var compName = _componentFactory.GetComponentName<MCPlanetMapPrototypeComponent>();
        if (!ev.Prototype.TryGetComponent<MCPlanetMapPrototypeComponent>(compName, out var prototypeComponent))
            return;

        Load(ev.Entity, prototypeComponent);
    }

    public void Load(Entity<MapComponent> entity, MCPlanetMapPrototypeComponent prototypeComponent)
    {
        if (prototypeComponent.MapsAbove.Count == 0 && prototypeComponent.MapsBelow.Count == 0)
            return;

        var stationNetwork = _zLevel.CreateZNetwork();
        _meta.SetEntityName(stationNetwork, $"Station z-Network: {MetaData(entity).EntityName}");

        var dict = new Dictionary<EntityUid, int> { { entity, 0 } };
        EntityManager.AddComponents(entity, prototypeComponent.ZLevelsComponentOverrides);

        // Loading maps below first
        var depth = prototypeComponent.MapsBelow.Count * -1;
        foreach (var mapBelow in prototypeComponent.MapsBelow)
        {
            if (!_mapLoader.TryLoadMap(mapBelow, out var mapEnt, out _))
            {
                Log.Error($"Failed to load map for Station zNetwork at depth {depth}!");
                continue;
            }

            Log.Info($"Created map {mapEnt.Value.Comp.MapId} for Station zNetwork at level {depth}");
            EntityManager.AddComponents(mapEnt.Value, prototypeComponent.ZLevelsComponentOverrides);
            _map.InitializeMap(mapEnt.Value.Comp.MapId);
            _meta.SetEntityName(mapEnt.Value, $"{MetaData(entity).EntityName} [{depth}]");
            dict.Add(mapEnt.Value, depth);
            depth++;
        }

        // Loading maps above next
        depth = 1;
        foreach (var mapAbove in prototypeComponent.MapsAbove)
        {
            if (!_mapLoader.TryLoadMap(mapAbove, out var mapEnt, out _))
            {
                Log.Error($"Failed to load map for Station zNetwork at depth {depth}!");
                continue;
            }

            Log.Info($"Created map {mapEnt.Value.Comp.MapId} for Station zNetwork at level {depth}");
            EntityManager.AddComponents(mapEnt.Value, prototypeComponent.ZLevelsComponentOverrides);
            _map.InitializeMap(mapEnt.Value.Comp.MapId);
            _meta.SetEntityName(mapEnt.Value, $"{MetaData(entity).EntityName} [{depth}]");
            dict.Add(mapEnt.Value, depth);
            depth++;
        }

        _zLevel.TryAddMapsIntoZNetwork(stationNetwork, dict);
    }
}
