using Content.Server._MC.Xeno.Hive;
using Content.Server._MC.Xeno.Spawn;
using Content.Server.Administration.Managers;
using Content.Server.GameTicking;
using Content.Server.Mind;
using Content.Server.Players.PlayTimeTracking;
using Content.Server.Preferences.Managers;
using Content.Server.RoundEnd;
using Content.Shared._MC.Nuke.Bomb.Events;
using Content.Shared._MC.Rules;
using Content.Shared._MC.Xeno.Hive.Components;
using Content.Shared._MC.Xeno.Hive.Events;
using Content.Shared._RMC14.Spawners;
using Content.Shared._RMC14.Xenonids;
using Content.Shared._RMC14.Xenonids.Evolution;
using Content.Shared._RMC14.Xenonids.Hive;
using Content.Shared.Coordinates;
using Content.Shared.GameTicking.Components;
using Content.Shared.Preferences;
using Content.Shared.Roles;
using Robust.Server.Player;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Server._MC.Rules.Distress;

public sealed partial class MCDistressRuleSystem : MCRuleSystem<MCDistressSignalRuleComponent>
{
    [Dependency] private readonly IBanManager _bans = null!;
    [Dependency] private readonly IPlayerManager _player = null!;
    [Dependency] private readonly IRobustRandom _random = null!;
    [Dependency] private readonly IServerPreferencesManager _preferences = null!;
    [Dependency] private readonly RoundEndSystem _roundEnd = null!;

    [Dependency] private readonly XenoSystem _rmcXeno = null!;
    [Dependency] private readonly SharedXenoHiveSystem _rmcHive = null!;
    [Dependency] private readonly XenoEvolutionSystem _rmcEvolution = null!;

    [Dependency] private readonly PlayTimeTrackingSystem _playTime = null!;
    [Dependency] private readonly MindSystem _mind = null!;

    [Dependency] private readonly MCXenoHiveSystem _mcXenoHive = null!;
    [Dependency] private readonly MCXenoSpawnSystem _mcXenoSpawn = null!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<LoadingMapsEvent>(OnMapLoading);
        SubscribeLocalEvent<RulePlayerSpawningEvent>(OnRulePlayerSpawning);

        SubscribeLocalEvent<MCNukeExplodedEvent>(OnNukeExploded);
        SubscribeLocalEvent<MCXenoHiveCollapsed>(OnHiveCollapsed);
    }

    protected override void OnStartAttempt(Entity<MCDistressSignalRuleComponent, GameRuleComponent> gameRule, RoundStartAttemptEvent ev)
    {
        if (ev.Forced || ev.Cancelled)
            return;

        var query = QueryAllRules();
        while (query.MoveNext(out _, out var ruleComponent, out _))
        {
            var xenoCandidates = 0;
            foreach (var player in ev.Players)
            {
                if (!_preferences.TryGetCachedPreferences(player.UserId, out var preferences))
                    continue;

                var profile = (HumanoidCharacterProfile) preferences.GetProfile(preferences.SelectedCharacterIndex);
                if (profile.JobPriorities.TryGetValue(ruleComponent.XenoSelectableJob, out var xenoPriority) && xenoPriority > JobPriority.Never ||
                    profile.JobPriorities.TryGetValue(ruleComponent.ShrikeJob, out var shrikePriority) && shrikePriority > JobPriority.Never)
                    xenoCandidates++;
            }

            if (ev.Players.Length <= 2)
            {
                Announce($"Невозможно запустить крушение. Требуется как минимум 2 игрока, но у нас есть {ev.Players.Length}.");
                ev.Cancel();
            }

            if (xenoCandidates >= 1)
                continue;

            Announce($"Невозможно запустить крушение. Требуется как минимум 1 ксено-игрок, но у нас есть {xenoCandidates}.");
            ev.Cancel();
        }

        return;

        void Announce(string msg)
        {
            ChatManager.SendAdminAnnouncement(msg);
            ChatManager.DispatchServerAnnouncement(msg);
        }
    }

    private void OnMapLoading(LoadingMapsEvent ev)
    {
        if (!GameTicker.IsGameRuleAdded<MCDistressSignalRuleComponent>())
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

            // Hive settings
            if (_mcXenoHive.DefaultHive is { } defaultHive)
            {
                var configuration = new MCXenoHiveConfiguration
                {
                    General = new MCXenoHiveConfigGeneral
                    {
                        AllowCollapse = true,
                        AllowHarvestLarvaPoints = true,
                        AllowGenerateLarvaPoint = true,
                    },
                    Evolution = new MCXenoHiveConfigEvolution
                    {
                        WithoutRuler = false,
                        RequiredCasteCount =
                        {
                            { "MCXenoQueen", 6 },
                            { "MCXenoKing", 12 },
                        },
                    },
                };

                _mcXenoHive.SetConfiguration(defaultHive, configuration);

                _mcXenoHive.AddPsypoints(defaultHive, "Strategic", 1600);
                _mcXenoHive.AddPsypoints(defaultHive, "Tactical", 400);
            }

            StartBioscan();

            SpawnAdminAreas(comp.Thunderdome);
            // SpawnNukeDiskGenerators();

            RefreshIFF(comp.MarineFaction);
            RefreshFaxes();

          var xenoSpawnPoints = GetEntities<XenoSpawnPointComponent>();

            var xenos = GetXenos(ev.PlayerPool.Count);
            // var survivors = GetSurvivors(ev.PlayerPool.Count);
            // var marines = GetMarines(ev.PlayerPool.Count);

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

                if (TryComp<XenoEvolutionComponent>(xenoEnt, out var xenoEvolution))
                    _rmcEvolution.SetPoints((xenoEnt, xenoEvolution), 100);

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

    private void OnNukeExploded(MCNukeExplodedEvent ev)
    {
        foreach (var gameRule in GameTicker.GetActiveGameRules())
        {
            if (!TryComp<MCDistressSignalRuleComponent>(gameRule, out var component))
                continue;

            EndRound((gameRule, component), MCDisstressRuleResult.MajorMarineVictory);
        }
    }

    private void OnHiveCollapsed(ref MCXenoHiveCollapsed ev)
    {
        foreach (var gameRule in GameTicker.GetActiveGameRules())
        {
            if (!TryComp<MCDistressSignalRuleComponent>(gameRule, out var component))
                continue;

            EndRound((gameRule, component), MCDisstressRuleResult.MajorMarineVictory);
        }
    }
}
