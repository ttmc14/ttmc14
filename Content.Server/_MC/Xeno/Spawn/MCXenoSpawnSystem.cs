using System.Linq;
using Content.Server._MC.Xeno.Hive;
using Content.Server._RMC14.MapInsert;
using Content.Server._RMC14.Xenonids.Hive;
using Content.Shared._MC.Rules;
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

public sealed class MCXenoSpawnSystem : EntitySystem
{
    [Dependency] private readonly IConfigurationManager _config = default!;
    [Dependency] private readonly IRobustRandom _random = default!;

    [Dependency] private readonly MapSystem _map = default!;
    [Dependency] private readonly MapInsertSystem _mapInsert = default!;
    [Dependency] private readonly MapLoaderSystem _mapLoader = default!;

    [Dependency] private readonly RMCAmbientLightSystem _rmcAmbientLight = default!;
    [Dependency] private readonly RMCPlanetSystem _rmcPlanet = default!;
    [Dependency] private readonly XenoHiveSystem _rmcXenoHive = default!;

    [Dependency] private readonly MCXenoHiveSystem _mcXenoHive = default!;

    [ViewVariables]
    private readonly Queue<EntProtoId<RMCPlanetMapPrototypeComponent>> _lastPlanetMaps = new();

    [ViewVariables]
    private RMCPlanet? _selectedPlanetMap;

    [ViewVariables]
    private string? _activeNightmareScenario;

    private TimeSpan _sunsetDuration;
    private TimeSpan _sunriseDuration;
    private int _mapVoteExcludeLast;

    public override void Initialize()
    {
        base.Initialize();

        Subs.CVar(_config, RMCCVars.RMCSunsetDuration, v => _sunsetDuration = TimeSpan.FromSeconds(v), true);
        Subs.CVar(_config, RMCCVars.RMCSunriseDuration, v => _sunriseDuration = TimeSpan.FromSeconds(v), true);
        Subs.CVar(_config, RMCCVars.RMCPlanetMapVoteExcludeLast, v => _mapVoteExcludeLast = v, true);
    }

    public bool SpawnXenoMap<T>(Entity<T> rule) where T : Component, IXenoMapRuleComponent
    {
        var planet = SelectRandomPlanet();
        _lastPlanetMaps.Enqueue(planet.Proto.ID);
        while (_lastPlanetMaps.Count > 0 && _lastPlanetMaps.Count > _mapVoteExcludeLast)
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

        SetFriendlyHives(_mcXenoHive.DefaultHive);
        return true;
    }

    public RMCPlanet SelectRandomPlanet()
    {
        if (_selectedPlanetMap is not null)
            return _selectedPlanetMap.Value;

        var planet = _random.Pick(_rmcPlanet.GetCandidates());
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
