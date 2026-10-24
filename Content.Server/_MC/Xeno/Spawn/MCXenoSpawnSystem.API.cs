using System.Linq;
using Content.Server._RMC14.MapInsert;
using Content.Shared._MC.Map;
using Content.Shared._RMC14.Rules;
using Content.Shared._RMC14.TacticalMap;
using JetBrains.Annotations;
using Robust.Shared.Random;

namespace Content.Server._MC.Xeno.Spawn;

public sealed partial class MCXenoSpawnSystem
{
    [PublicAPI]
    private void ResetPlanet()
    {
        _selectedPlanetMap = null;
    }

    [PublicAPI]
    public void SetPlanet(RMCPlanet planet)
    {
        _selectedPlanetMap = planet;
    }

    [PublicAPI]
    public RMCPlanet GetRandomPlanet()
    {
        var candidates = _rmcPlanet.GetCandidatesInRotation();
        if (candidates.Count != 0)
            return _random.Pick(candidates);

        Log.Error("No planet candidates available for random selection.");
        throw new InvalidOperationException("No planets available.");
    }

    [PublicAPI]
    public RMCPlanet RequestPlanet()
    {
        if (_selectedPlanetMap is not null)
            return _selectedPlanetMap.Value;

        var map = GetRandomPlanet();
        _selectedPlanetMap = map;

        return map;
    }

    [PublicAPI]
    public bool SpawnMap(out EntityUid planetUid)
    {
        planetUid = default;

        var planet = RequestPlanet();

        // Manage history exclusion
        UpdatePlanetHistory(planet.Proto.ID);

        if (!TryLoadAndInitializeMap(planet, out var mapUid, out var gridUid))
        {
            Log.Error($"Failed to spawn map for planet {planet.Proto.ID}");
            return false;
        }

        planetUid = gridUid;

        // Post-spawn logic
        ProcessMapInsertions();
        RaisePlanetSpawnEvent(mapUid, planet);
        SetFriendlyHives(_mcXenoHive.DefaultHive);
        return true;
    }
}
