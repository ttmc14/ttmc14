using System.Diagnostics.CodeAnalysis;
using Content.Server._RMC14.MapInsert;
using Content.Server._RMC14.Xenonids.Hive;
using Content.Server.Administration.Logs;
using Content.Server.Administration.Managers;
using Content.Server.Fax;
using Content.Server.GameTicking.Rules;
using Content.Server.Mind;
using Content.Server.Players.PlayTimeTracking;
using Content.Server.Station.Systems;
using Content.Shared._MC.Nuke.Generator.Components;
using Content.Shared._RMC14.Bioscan;
using Content.Shared._RMC14.CCVar;
using Content.Shared._RMC14.Light;
using Content.Shared._RMC14.Rules;
using Content.Shared._RMC14.Thunderdome;
using Content.Shared._RMC14.Weapons.Ranged.IFF;
using Content.Shared._RMC14.Xenonids;
using Content.Shared._RMC14.Xenonids.Construction.Tunnel;
using Content.Shared._RMC14.Xenonids.Parasite;
using Content.Shared.Fax.Components;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Robust.Server.GameObjects;
using Robust.Server.Player;
using Robust.Shared.Configuration;
using Robust.Shared.EntitySerialization.Systems;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Utility;

namespace Content.Server._MC.Rules;

// TODO: I HATE THIS SHIIIT
public abstract partial class MCRuleSystem<T> : GameRuleSystem<T> where T : IComponent
{
    [Dependency] protected readonly XenoHiveSystem Hive = default!;

    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly RMCPlanetSystem _rmcPlanet = default!;
    [Dependency] private readonly MapLoaderSystem _mapLoader = default!;
    [Dependency] private readonly MapSystem _mapSystem = default!;
    [Dependency] private readonly MapInsertSystem _mapInsert = default!;
    [Dependency] private readonly RMCAmbientLightSystem _rmcAmbientLight = default!;
    [Dependency] private readonly IConfigurationManager _config = default!;
    [Dependency] private readonly XenoTunnelSystem _xenoTunnel = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly IComponentFactory _compFactory = default!;
    [Dependency] private readonly StationSpawningSystem _stationSpawning = default!;
    [Dependency] private readonly SharedXenoParasiteSystem _parasite = default!;
    [Dependency] private readonly XenoSystem _xeno = default!;
    [Dependency] private readonly IAdminLogManager _adminLog = default!;
    [Dependency] private readonly FaxSystem _fax = default!;
    [Dependency] private readonly GunIFFSystem _gunIFF = default!;
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly PlayTimeTrackingSystem _playTime = default!;
    [Dependency] private readonly IBanManager _bans = default!;
    [Dependency] private readonly MindSystem _mind = default!;

    [ViewVariables] public string? OperationName { get; protected set; }

    private readonly HashSet<string> _operationNames = new();
    private readonly HashSet<string> _operationPrefixes = new();
    private readonly HashSet<string> _operationSuffixes = new();

    protected float MarinesPerXeno;
    protected float MarinesPerSurvivor;
    protected float MinimumSurvivors;
    protected float MaximumSurvivors;

    private bool _usingCustomOperationName;
    private int _mapVoteExcludeLast;
    private string _adminFaxAreaMap = string.Empty;

    public override void Initialize()
    {
        base.Initialize();

        Subs.CVar(_config, RMCCVars.CMMarinesPerXeno, v => MarinesPerXeno = v, true);
        Subs.CVar(_config, RMCCVars.RMCMarinesPerSurvivor, v => MarinesPerSurvivor = v, true);
        Subs.CVar(_config, RMCCVars.RMCSurvivorsMinimum, v => MinimumSurvivors = v, true);
        Subs.CVar(_config, RMCCVars.RMCSurvivorsMaximum, v => MaximumSurvivors = v, true);
        Subs.CVar(_config, RMCCVars.RMCAdminFaxAreaMap, v => _adminFaxAreaMap = v, true);
    }

    protected int GetXenos(int players)
    {
        return (int) Math.Round(Math.Max(1, players / MarinesPerXeno));
    }

    protected int GetSurvivors(int players)
    {
        return (int) Math.Clamp(Math.Round(Math.Max(1, players / MarinesPerXeno)), MinimumSurvivors, MaximumSurvivors);
    }

    protected int GetMarines(int players)
    {
        return players - GetXenos(players) - GetSurvivors(players);
    }

    protected void SpawnAdminAreas(ResPath thunderdome)
    {
        SpawnMap(new ResPath(_adminFaxAreaMap), out _);

        if (SpawnMap(thunderdome, out var mapEnt))
            EnsureComp<ThunderdomeMapComponent>(mapEnt.Value);

        return;

        bool SpawnMap(ResPath path, [NotNullWhen(true)] out EntityUid? mapEnt)
        {
            mapEnt = default;

            try
            {
                if (string.IsNullOrWhiteSpace(path.ToString()))
                    return false;

                if (!_mapLoader.TryLoadMap(path, out var map, out _))
                    return false;

                _mapSystem.InitializeMap((map.Value, map.Value));
                mapEnt = map;
                return true;
            }
            catch (Exception exception)
            {
                Log.Error($"Error loading admin fax area:\n{exception}");
            }

            return false;
        }
    }

    protected string GetRandomOperationName()
    {
        if (_usingCustomOperationName && OperationName is not null)
        {
            _usingCustomOperationName = false;
            return OperationName;
        }

        var name = string.Empty;
        if (_operationNames.Count > 0)
            name += $"{_random.Pick(_operationNames)} ";

        if (_operationPrefixes.Count > 0)
            name += $"{_random.Pick(_operationPrefixes)}";

        if (_operationSuffixes.Count > 0)
            name += $"-{_random.Pick(_operationSuffixes)}";

        return name.Trim();
    }

    protected void StartBioscan()
    {
        EnsureComp<BioscanComponent>(Spawn(null, MapCoordinates.Nullspace));
    }

    protected void RefreshIFF(EntProtoId<IFFFactionComponent> faction)
    {
        var marineFactions = EntityQueryEnumerator<MarineIFFComponent>();
        while (marineFactions.MoveNext(out var iffId, out _))
        {
            _gunIFF.SetUserFaction(iffId, faction);
        }
    }

    protected void RefreshFaxes()
    {
        var faxes = EntityQueryEnumerator<FaxMachineComponent>();
        while (faxes.MoveNext(out var faxId, out var faxComp))
        {
            _fax.Refresh(faxId, faxComp);
        }
    }

    protected void SpawnNukeDiskGenerators()
    {
        var protoIds = new EntProtoId[]
        {
            "MCComputerNukeDiskGeneratorRed",
            "MCComputerNukeDiskGeneratorGreen",
            "MCComputerNukeDiskGeneratorBlue",
        };

        var coordinates = new List<MapCoordinates>();
        var query = EntityQueryEnumerator<MCNukeDiskGeneratorSpawnerComponent>();
        while (query.MoveNext(out var uid, out _))
        {
            coordinates.Add(_transform.GetMapCoordinates(uid));
        }

        foreach (var protoId in protoIds)
        {
            if (coordinates.Count == 0)
            {
                Log.Error($"Failed to spawn {protoId}, no available coordinates. Ensure that MCNukeDiskGeneratorSpawnerComponent exists on the map.");
                break;
            }

            Spawn(protoId, _random.PickAndTake(coordinates));
        }
    }

    protected int GetLiving<T>() where T : IComponent
    {
        var total = 0;
        var query = EntityQueryEnumerator<T, MobStateComponent>();
        while (query.MoveNext(out _, out _, out var mobStateComponent))
        {
            if (mobStateComponent.CurrentState == MobState.Dead)
                continue;

            total++;
        }

        return total;
    }
}
