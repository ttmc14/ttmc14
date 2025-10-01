using Content.Server._MC.Xeno.Hive;
using Content.Server._MC.Xeno.Spawn;
using Content.Server.Administration.Managers;
using Content.Server.GameTicking;
using Content.Server.Maps;
using Content.Server.Mind;
using Content.Server.Players.PlayTimeTracking;
using Content.Server.Shuttles.Systems;
using Content.Shared._MC.Rules;
using Content.Shared._RMC14.Spawners;
using Content.Shared._RMC14.Xenonids;
using Content.Shared._RMC14.Xenonids.Hive;
using Content.Shared.Coordinates;
using Content.Shared.Preferences;
using Content.Shared.Roles;
using Robust.Server.Player;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Server._MC.Rules;

public sealed class MCDistressSignalRuleSystem : MCRuleSystem<MCDistressSignalRuleComponent>
{
    [Dependency] private readonly IBanManager _bans = default!;
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly IRobustRandom _random = default!;

    [Dependency] private readonly XenoSystem _rmcXeno = default!;
    [Dependency] private readonly SharedXenoHiveSystem _rmcHive = default!;

    [Dependency] private readonly PlayTimeTrackingSystem _playTime = default!;
    [Dependency] private readonly ShuttleSystem _shuttle = default!;
    [Dependency] private readonly MindSystem _mind = default!;

    [Dependency] private readonly MCXenoHiveSystem _mcXenoHive = default!;
    [Dependency] private readonly MCXenoSpawnSystem _mcXenoSpawn = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<LoadingMapsEvent>(OnMapLoading);
        SubscribeLocalEvent<RulePlayerSpawningEvent>(OnRulePlayerSpawning);
    }

    private void OnMapLoading(LoadingMapsEvent ev)
    {
        if (!GameTicker.IsGameRuleAdded<MCCrashRuleComponent>())
            return;

        _mcXenoSpawn.SelectRandomPlanet();
        GameTicker.UpdateInfoText();
    }

    private void OnRulePlayerSpawning(RulePlayerSpawningEvent ev)
    {
        var query = QueryActiveRules();
        while (query.MoveNext(out var uid, out _, out var comp, out var gameRule))
        {
            if (!GameTicker.IsGameRuleAdded(uid, gameRule))
                continue;

            OperationName = GetRandomOperationName();
            if (!_mcXenoSpawn.SpawnXenoMap<MCDistressSignalRuleComponent>((uid, comp)))
                continue;

            StartBioscan();

            SpawnAdminAreas(comp.Thunderdome);
            SpawnNukeDiskGenerators();

            RefreshIFF(comp.MarineFaction);
            RefreshFaxes();

            var xenoSpawnPoints = GetEntities<XenoSpawnPointComponent>();

            var xenos = GetXenos(ev.PlayerPool.Count);
            var survivors = GetSurvivors(ev.PlayerPool.Count);
            var marines = GetMarines(ev.PlayerPool.Count);

            var priorities = Enum.GetValues<JobPriority>().Length;
            var xenoCandidates = new List<NetUserId>[priorities];
            for (var i = 0; i < xenoCandidates.Length; i++)
            {
                xenoCandidates[i] = [];
            }

            foreach (var (id, profile) in ev.Profiles)
            {
                if (!IsAllowed(id, comp.QueenJob))
                    continue;

                if (profile.JobPriorities.TryGetValue(comp.QueenJob, out var priority) && priority > JobPriority.Never)
                    xenoCandidates[(int) priority].Add(id);
            }

            NetUserId? queenSelected = null;
            NetUserId? shrikeSelected = null;

            if (xenos > 8)
            {
                for (var i = xenoCandidates.Length - 1; i >= 0; i--)
                {
                    var list = xenoCandidates[i];
                    while (list.Count > 0)
                    {
                        queenSelected = SpawnXeno(list, comp.QueenEnt);

                        if (queenSelected is not null)
                            break;
                    }

                    if (queenSelected is null)
                        continue;

                    xenos--;
                    break;
                }
            }

            foreach (var list in xenoCandidates)
            {
                list.Clear();
            }

            foreach (var (id, profile) in ev.Profiles)
            {
                if (id == queenSelected)
                    continue;

                if (!IsAllowed(id, comp.ShrikeJob))
                    continue;

                if (profile.JobPriorities.TryGetValue(comp.ShrikeJob, out var priority) && priority > JobPriority.Never)
                {
                    xenoCandidates[(int) priority].Add(id);
                }
            }

            for (var i = xenoCandidates.Length - 1; i >= 0; i--)
            {
                var list = xenoCandidates[i];
                while (list.Count > 0)
                {
                    shrikeSelected = SpawnXeno(list, comp.ShrikeEnt);
                    if (shrikeSelected is not null)
                        break;
                }

                if (shrikeSelected is null)
                    continue;

                xenos--;
                break;
            }

            foreach (var list in xenoCandidates)
            {
                list.Clear();
            }

            foreach (var (id, profile) in ev.Profiles)
            {
                if (id == queenSelected || id == shrikeSelected)
                    continue;

                if (!IsAllowed(id, comp.XenoSelectableJob))
                    continue;

                if (profile.JobPriorities.TryGetValue(comp.XenoSelectableJob, out var priority) && priority > JobPriority.Never)
                    xenoCandidates[(int) priority].Add(id);
            }

            var selectedXenos = 0;
            for (var i = xenoCandidates.Length - 1; i >= 0; i--)
            {
                var list = xenoCandidates[i];
                while (list.Count > 0 && selectedXenos < xenos)
                {
                    if (SpawnXeno(list, comp.LarvaEnt) != null)
                        selectedXenos++;
                }
            }

            // Any unfilled xeno slots become larva
            var unfilled = xenos - selectedXenos;
            if (unfilled > 0)
                _rmcHive.IncreaseBurrowedLarva(unfilled);

            continue;

            bool IsAllowed(NetUserId id, ProtoId<JobPrototype> role)
            {
                if (!_player.TryGetSessionById(id, out var player))
                    return false;

                var jobBans = _bans.GetJobBans(player.UserId);
                if (jobBans is null || jobBans.Contains(role))
                    return false;

                return _playTime.IsAllowed(player, role);
            }

            NetUserId? SpawnXeno(List<NetUserId> list, EntProtoId ent)
            {
                var playerId = _random.PickAndTake(list);
                if (!_player.TryGetSessionById(playerId, out var player))
                {
                    Log.Error($"Failed to find player with id {playerId} during xeno selection.");
                    return null;
                }

                ev.PlayerPool.Remove(player);
                GameTicker.PlayerJoinGame(player);
                var xenoEnt = SpawnXenoEnt(ent);

                if (!_mind.TryGetMind(playerId, out var mind))
                    mind = _mind.CreateMind(playerId);

                _mind.TransferTo(mind.Value, xenoEnt);
                return playerId;
            }

            EntityUid SpawnXenoEnt(EntProtoId ent)
            {
                var point = _random.Pick(xenoSpawnPoints);
                var xenoEnt = SpawnAtPosition(ent, point.ToCoordinates());

                _rmcXeno.MakeXeno(xenoEnt);
                _rmcHive.SetHive(xenoEnt, _mcXenoHive.DefaultHive);
                return xenoEnt;
            }
        }
    }

    private void CheckRoundShouldEnd()
    {

    }
}
