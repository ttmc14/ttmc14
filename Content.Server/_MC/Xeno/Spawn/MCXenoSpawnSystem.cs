using System.Linq;
using Content.Server._MC.Xeno.Hive;
using Content.Server._RMC14.MapInsert;
using Content.Server._RMC14.Xenonids.Hive;
using Content.Shared._MC.Map;
using Content.Shared._MC.Rules.Base;
using Content.Shared._RMC14.CCVar;
using Content.Shared._RMC14.Light;
using Content.Shared._RMC14.Rules;
using Content.Shared._RMC14.TacticalMap;
using Content.Shared._RMC14.Xenonids;
using Robust.Server.GameObjects;
using Robust.Shared.Configuration;
using Robust.Shared.EntitySerialization.Systems;
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

    [Dependency] private readonly RMCAmbientLightSystem _rmcAmbientLight = null!;
    [Dependency] private readonly RMCPlanetSystem _rmcPlanet = null!;
    [Dependency] private readonly XenoHiveSystem _rmcXenoHive = null!;

    [Dependency] private readonly MCXenoHiveSystem _mcXenoHive = null!;

    [ViewVariables]
    private readonly Queue<EntProtoId<RMCPlanetMapPrototypeComponent>> _lastPlanetMaps = new();

    [ViewVariables]
    private RMCPlanet? _selectedPlanetMap;

    [ViewVariables]
    private string? _activeNightmareScenario;

    private TimeSpan _sunsetDuration;
    private TimeSpan _sunriseDuration;

    public override void Initialize()
    {
        base.Initialize();

        InitializeVote();

        Subs.CVar(_config, RMCCVars.RMCSunsetDuration, v => _sunsetDuration = TimeSpan.FromSeconds(v), true);
        Subs.CVar(_config, RMCCVars.RMCSunriseDuration, v => _sunriseDuration = TimeSpan.FromSeconds(v), true);
    }

    public bool SpawnXenoMap<T>(Entity<T> rule) where T : Component, IRulePlanet
    {
        var planet = SelectRandomPlanet();

        _lastPlanetMaps.Enqueue(planet.Proto.ID);
        while (_lastPlanetMaps.Count > 0 && _lastPlanetMaps.Count > _voteExcludeLast)
        {
            _lastPlanetMaps.Dequeue();
        }

        if (!_mapLoader.TryLoadMap(planet.Comp.Map, out var mapNullable, out var grids))
        {
            Log.Error("Failed to load xeno map");
            return false;
        }

        var map = mapNullable.Value;
        EnsureComp<RMCPlanetComponent>(map);
        EnsureComp<TacticalMapComponent>(map);

        switch (grids.Count)
        {
            case 0:
                Log.Error("Failed to load xeno map");
                return false;

            case > 1:
                Log.Error("Multiple planet-side grids found");
                break;
        }

        rule.Comp.XenoMap = grids.First();

        _map.InitializeMap((map, map));

        _activeNightmareScenario = string.Empty;
        if (_selectedPlanetMap?.Comp.NightmareScenarios is not null)
            _activeNightmareScenario = _mapInsert.SelectMapScenario(_selectedPlanetMap.Value.Comp.NightmareScenarios);

        var mapInsertQuery = EntityQueryEnumerator<MapInsertComponent>();
        while (mapInsertQuery.MoveNext(out var uid, out var mapInsert))
        {
            _mapInsert.ProcessMapInsert((uid, mapInsert));
        }

        var xenoMap = rule.Comp.XenoMap.Value;

        var rmcAmbientComp = EnsureComp<RMCAmbientLightComponent>(xenoMap);
        var rmcAmbientEffectComp = EnsureComp<RMCAmbientLightEffectsComponent>(xenoMap);

        var colorSequence = _rmcAmbientLight.ProcessPrototype(rmcAmbientEffectComp.Sunset);
        _rmcAmbientLight.SetColor((xenoMap, rmcAmbientComp), colorSequence, _sunsetDuration);

        var ev = new MCPlanetMapSpawnEvent(map, planet.Proto);
        RaiseLocalEvent(ref ev);

        SetFriendlyHives(_mcXenoHive.DefaultHive);

        return true;
    }

    public RMCPlanet SelectRandomPlanet()
    {
        if (_selectedPlanetMap is not null)
            return _selectedPlanetMap.Value;

        var planet = _random.Pick(_rmcPlanet.GetCandidatesInRotation());
        _selectedPlanetMap = planet;
        return planet;
    }

    private void ResetSelectedPlanet()
    {
        _selectedPlanetMap = null;
    }

    public void SetPlanet(RMCPlanet planet)
    {
        _selectedPlanetMap = planet;
    }

    private void SetFriendlyHives(EntityUid? uid)
    {
        if (!Exists(uid))
            return;

        var query = EntityQueryEnumerator<XenoFriendlyComponent>();
        while (query.MoveNext(out var weeds, out _))
        {
            _rmcXenoHive.SetHive(weeds, uid);
        }
    }
}
