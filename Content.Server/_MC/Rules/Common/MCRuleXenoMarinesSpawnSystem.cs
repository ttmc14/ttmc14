using System.Linq;
using Content.Server.GameTicking;
using Content.Server.GameTicking.Rules;
using Content.Server.Station.Components;
using Content.Shared._MC.Rules.Common;
using Content.Shared.Preferences;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Random;

namespace Content.Server._MC.Rules.Common;

public sealed partial class MCRuleXenoMarinesSpawnSystem : GameRuleSystem<MCRuleXenoMarinesSpawnComponent>
{
    [Dependency] private readonly IRobustRandom _random = null!;
    [Dependency] private readonly MCRelayedSystem _mcRelayed = null!;

    public bool Spawned
    {
        get
        {
            var query = QueryActiveRules();
            while (query.MoveNext(out var uid, out _, out var component, out var gameRule))
            {
                if (!GameTicker.IsGameRuleAdded(uid, gameRule))
                    continue;

                if (!component.Spawned)
                    return false;
            }

            return true;
        }
    }

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<RulePlayerSpawningEvent>(OnRulePlayerSpawning);
    }


    private void OnRulePlayerSpawning(RulePlayerSpawningEvent ev)
    {
        var query = QueryActiveRules();
        while (query.MoveNext(out var uid, out _, out var component, out var gameRule))
        {
            if (!GameTicker.IsGameRuleAdded(uid, gameRule))
                continue;

            DistributePlayersAndSpawn((uid, component), ev);
        }
    }

    private void DistributePlayersAndSpawn(Entity<MCRuleXenoMarinesSpawnComponent> entity, RulePlayerSpawningEvent ev)
    {
        // Data is copied to avoid mutating the events of the original collection
        var players = new List<ICommonSession>(ev.PlayerPool);
        var profiles = new Dictionary<NetUserId, HumanoidCharacterProfile>(ev.Profiles);

        ev.PlayerPool.Clear();

        _mcRelayed.Relay(() =>
        {
            var assignedXenos = AssignXenoRoles(entity, profiles, players);
            var marineProfiles = profiles
                .Where(p => !assignedXenos.Contains(p.Key))
                .ToDictionary(p => p.Key, p => p.Value);

            SpawnMarines(marineProfiles);

            entity.Comp.Spawned = true;
            Dirty(entity);
        });
    }

    private IReadOnlyList<NetUserId> AssignXenoRoles(
        Entity<MCRuleXenoMarinesSpawnComponent> entity,
        Dictionary<NetUserId, HumanoidCharacterProfile> profiles,
        List<ICommonSession> players)
    {
        var remainingSlots = CalculateXenoCount(players.Count);

        var uniqueXenos = AssignUniqueXenoRoles(entity.Comp.XenoUnique, profiles, ref remainingSlots);
        var overflowXenos = AssignOverflowXenoRoles(entity.Comp.XenoOverflow, profiles, ref remainingSlots);

        return [.. uniqueXenos.Union(overflowXenos)];
    }

    private IReadOnlyList<NetUserId> AssignUniqueXenoRoles(
        IReadOnlyList<MCJobRolePair> uniqueRoles,
        Dictionary<NetUserId, HumanoidCharacterProfile> profiles,
        ref int remainingSlots)
    {
        var assignedPlayers = new List<NetUserId>();

        foreach (var role in uniqueRoles)
        {
            if (remainingSlots <= 0)
                break;

            var id = TryAssignSingleXenoRole(role, profiles, remainingSlots);
            if (id is null)
                continue;

            assignedPlayers.Add(id.Value);
            remainingSlots--;
        }

        return assignedPlayers;
    }

    private NetUserId? TryAssignSingleXenoRole(
        MCJobRolePair role,
        Dictionary<NetUserId, HumanoidCharacterProfile> profiles,
        int remainingSlots)
    {
        var id = SelectPlayerForJob(role.JobId, profiles);
        if (id is null)
            return null;

        return TrySpawnXeno(id.Value, role.EntityId, remainingSlots)
            ? id
            : null;
    }

    private void SpawnMarines(Dictionary<NetUserId, HumanoidCharacterProfile> profiles)
    {
        if (profiles.Count == 0)
            return;

        var spawnableStations = GetSpawnableStations();

        // Назначаем основные и резервные работы
        var assignedJobs = _stationJobs.AssignJobs(profiles, spawnableStations);
        var playerNetIds = profiles.Keys.ToHashSet();
        _stationJobs.AssignOverflowJobs(ref assignedJobs, playerNetIds, profiles, spawnableStations);

        // Считаем количество работ для расчета расширенного доступа
        var stationJobCounts = spawnableStations.ToDictionary(e => e, _ => 0);
        foreach (var (_, (job, station)) in assignedJobs)
        {
            if (job != null)
                stationJobCounts[station] += 1;
        }
        _stationJobs.CalcExtendedAccess(stationJobCounts);

        // Спавним игроков
        foreach (var (playerId, (jobId, station)) in assignedJobs)
        {
            if (!_players.TryGetSessionById(playerId, out var session))
                continue;

            if (jobId is null)
            {
                _gameTicker.JoinAsObserver(session);
                continue;
            }

            _gameTicker.MakeJoinGame(session, station, jobId);
        }
    }

    private List<EntityUid> GetSpawnableStations()
    {
        var stations = new List<EntityUid>();
        var query = EntityQueryEnumerator<StationJobsComponent, StationSpawningComponent>();

        while (query.MoveNext(out var uid, out _, out _))
        {
            stations.Add(uid);
        }

        return stations;
    }

    private static int CalculateXenoCount(int totalPlayers)
    {
        return (int) float.Round(float.Max(1f, totalPlayers / 3f));
    }
}
