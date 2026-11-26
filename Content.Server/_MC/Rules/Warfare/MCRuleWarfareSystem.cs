using Content.Server._MC.Rules.EndRound;
using Content.Server._MC.Xeno.Hive;
using Content.Server.GameTicking;
using Content.Server.GameTicking.Rules;
using Content.Server.Shuttles.Events;
using Content.Shared._MC.Map;
using Content.Shared._MC.Rules.Common;
using Content.Shared._MC.Rules.Warfare;
using Content.Shared._MC.Xeno.Hive.Systems.Main;
using Content.Shared._RMC14.Marines;
using Content.Shared._RMC14.Thunderdome;
using Content.Shared._RMC14.Xenonids;
using Content.Shared.GameTicking;
using Content.Shared.GameTicking.Components;
using Content.Shared.Ghost;
using Content.Shared.Mind;
using Content.Shared.Mobs.Systems;
using Content.Shared.Roles.Jobs;
using Robust.Shared.Player;

namespace Content.Server._MC.Rules.Warfare;

public sealed class MCRuleWarfareSystem : GameRuleSystem<MCRuleWarfareComponent>
{
    private const int MarinesPerXeno = 3;
    private const int LivesPerPlayer = 3;
    private const int PointsPerLarva = 9;
    private static readonly TimeSpan BalanceInterval = TimeSpan.FromSeconds(15);

    [Dependency] private readonly ISharedPlayerManager _players = null!;
    [Dependency] private readonly MobStateSystem _mobState = null!;
    [Dependency] private readonly SharedMindSystem _mind = null!;
    [Dependency] private readonly SharedJobSystem _jobs = null!;
    [Dependency] private readonly MCXenoHiveSystem _hive = null!;
    [Dependency] private readonly MCRuleEndRoundSystem _roundEnd = null!;
    [Dependency] private readonly MCWarfareSystem _warfare = null!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MCPlanetMapSpawnEvent>(OnPlanetSpawned);
        SubscribeLocalEvent<AlmayerComponent, FTLCompletedEvent>(OnShuttleLanded);
        SubscribeLocalEvent<PlayerBeforeSpawnEvent>(OnBeforeSpawn);
        SubscribeLocalEvent<PlayerSpawnCompleteEvent>(OnSpawnComplete);
    }

    private void OnPlanetSpawned(ref MCPlanetMapSpawnEvent args)
    {
        var query = QueryActiveRules();
        if (!query.MoveNext(out var uid, out _, out _, out _) || !TryComp<MCRuleCrashShuttleComponent>(uid, out var shuttle))
            return;

        ChatManager.DispatchServerAnnouncement(Loc.GetString("mc-warfare-deployment",
            ("planet", args.Prototype.Name), ("minutes", shuttle.Time.TotalMinutes)));
    }

    private void OnShuttleLanded(Entity<AlmayerComponent> entity, ref FTLCompletedEvent args)
    {
        var query = QueryActiveRules();
        while (query.MoveNext(out var uid, out _, out var rule, out _))
        {
            if (CompOrNull<MCRuleXenoMapComponent>(uid)?.Map is not { } planetGrid || Transform(planetGrid).MapUid != args.MapUid)
                continue;

            rule.Landed = true;
        }
    }

    private void OnBeforeSpawn(PlayerBeforeSpawnEvent args)
    {
        if (args.LateJoin && _warfare.ActiveRule is { MarineReserve: <= 0 })
        {
            args.SpawnError = "mc-warfare-no-marines";
        }
    }

    private void OnSpawnComplete(PlayerSpawnCompleteEvent args)
    {
        if (args.LateJoin && _warfare.ActiveRule is { } rule)
        {
            rule.MarineReserve--;
            GameTicker.UpdateInfoText();
        }
    }

    protected override void ActiveTick(EntityUid uid, MCRuleWarfareComponent rule, GameRuleComponent gameRule, float frameTime)
    {
        if (GameTicker.RunLevel != GameRunLevel.InRound || rule.NextBalance > Timing.CurTime ||
            !TryComp<MCRuleXenoMarinesSpawnComponent>(uid, out var spawn) || !spawn.Spawned)
            return;

        rule.NextBalance = Timing.CurTime + BalanceInterval;
        if (_hive.DefaultHive is not { } hive)
            return;

        var marinePoints = GetMarineLarvaPoints(rule, out var livingMarines);
        var livingXenos = _hive.GetLivingXenos(hive, minTier: 0);
        if (GameTicker.RoundDuration() >= TimeSpan.FromSeconds(5) && (livingMarines == 0 || livingXenos == 0))
        {
            var result = (livingMarines, livingXenos) switch
            {
                (0, 0) => "Draw",
                (0, _) => "MajorXenoVictory",
                _ => "MajorMarineVictory",
            };
            _roundEnd.EndRound(result);
            return;
        }

        var groups = (_players.PlayerCount - rule.PopulationThreshold) / (MarinesPerXeno + 1);
        if (groups > 0)
        {
            rule.PopulationThreshold += groups * (MarinesPerXeno + 1);
            rule.XenoReserve += groups * LivesPerPlayer;
            rule.MarineReserve += groups * LivesPerPlayer * MarinesPerXeno;
        }

        var granted = GrantLarvae(rule, hive, marinePoints, livingXenos);
        if (groups > 0 || granted > 0)
        {
            GameTicker.UpdateInfoText();
        }
    }

    private int GrantLarvae(MCRuleWarfareComponent rule, EntityUid hive, float marinePoints, int livingXenos)
    {
        if (rule.XenoReserve <= 0)
            return 0;

        var target = Math.Max(1, (int) (marinePoints / PointsPerLarva));
        var xenos = livingXenos + _hive.GetBurrowedLarva(hive);
        var needed = target - xenos;
        if (needed <= 0)
            return 0;

        var granted = Math.Min(needed, rule.XenoReserve);
        rule.XenoReserve -= granted;
        _hive.AddBurrowedLarva(hive, granted);
        return granted;
    }

    private float GetMarineLarvaPoints(MCRuleWarfareComponent rule, out int livingMarines)
    {
        livingMarines = 0;
        var points = 0f;
        var marines = EntityQueryEnumerator<MarineComponent, ActorComponent>();
        while (marines.MoveNext(out var uid, out _, out _))
        {
            if (_mobState.IsDead(uid) || HasComp<ThunderdomeMapComponent>(Transform(uid).MapUid))
                continue;

            livingMarines++;
            var worth = MCSharedXenoHiveSystem.LarvaPointsRegular;
            if (_mind.TryGetMind(uid, out var mind, out _) && _jobs.MindTryGetJobId(mind, out var job) && job is { } jobId)
            {
                worth = rule.LarvaPointsByJob.GetValueOrDefault(jobId, worth);
            }
            points += worth;
        }
        return points;
    }

    /// <summary>
    /// Returns the reserve counts visible to the player. Lobby players and observers can see both sides.
    /// </summary>
    public string? GetReserveBriefing(EntityUid? viewer)
    {
        if (_warfare.ActiveRule is not { } rule)
            return null;

        if (viewer is null || HasComp<GhostComponent>(viewer))
        {
            return Loc.GetString("mc-warfare-reserve-marines", ("count", rule.MarineReserve)) + "\n" +
                Loc.GetString("mc-warfare-reserve-xenos", ("count", rule.XenoReserve));
        }

        return HasComp<XenoComponent>(viewer)
            ? Loc.GetString("mc-warfare-reserve-xenos", ("count", rule.XenoReserve))
            : Loc.GetString("mc-warfare-reserve-marines", ("count", rule.MarineReserve));
    }
}
