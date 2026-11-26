using Content.Server._MC.Rules.Crash;
using Content.Server.GameTicking;
using Content.Server.GameTicking.Rules;
using Content.Server.Shuttles.Components;
using Content.Server.Shuttles.Events;
using Robust.Server.Player;
using Content.Server.Shuttles.Systems;
using Content.Shared._MC.Rules.Common;
using Content.Shared._MC.Shuttle;
using Content.Shared._RMC14.Marines;
using Content.Shared.Whitelist;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Server._MC.Rules.Common;

public sealed class MCRuleCrashShuttleSystem : GameRuleSystem<MCRuleCrashShuttleComponent>
{
    [Dependency] private readonly IPrototypeManager _prototype = null!;
    [Dependency] private readonly IRobustRandom _random = null!;

    [Dependency] private readonly EntityWhitelistSystem _whitelist = null!;
    [Dependency] private readonly ShuttleSystem _shuttle = null!;

    [Dependency] private readonly MCRelayedSystem _relayed = null!;
    [Dependency] private readonly MCCrashMapVoteSystem _mapVote = null!;

    [Dependency] private readonly IPlayerManager _players = null!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<AlmayerComponent, FTLCompletedEvent>(OnLanded);
        SubscribeLocalEvent<LoadingMapsEvent>(OnMapLoading);
        SubscribeLocalEvent<RulePlayerSpawningEvent>(OnRulePlayerSpawning);
    }

    private void OnMapLoading(LoadingMapsEvent ev)
    {
        if (!GameTicker.IsGameRuleAdded<MCRuleCrashShuttleComponent>())
            return;

        ev.Maps.Clear();
        var ship = _mapVote.SelectedMap;
        if (GameTicker.IsGameRuleAdded<MCRuleCrashLandingComponent>())
        {
            ship = _players.PlayerCount >= 25 ? "MCCanterburyMothership" : "MCCanterbury";
        }

        ev.Maps.Add(_prototype.Index(ship));

        GameTicker.UpdateInfoText();
    }

    private void OnRulePlayerSpawning(RulePlayerSpawningEvent ev)
    {
        var query = QueryActiveRules();
        while (query.MoveNext(out var uid, out _, out var component, out var gameRule))
        {
            if (!GameTicker.IsGameRuleAdded(uid, gameRule))
                continue;

            RulePlayerSpawning((uid, component));
        }
    }

    private void RulePlayerSpawning(Entity<MCRuleCrashShuttleComponent> entity)
    {
        _relayed.Relay(() =>
        {
            Crash(entity);
        });
    }

    private void Crash(Entity<MCRuleCrashShuttleComponent> entity)
    {
        var crashPoints = GetPoints(entity);
        if (crashPoints.Count == 0)
        {
            Log.Fatal("Fuck!");
            return;
        }

        var targetUid = _random.Pick(crashPoints);
        var coordinates = Transform(targetUid).Coordinates.Offset(targetUid.Comp.Offset);

        var query = EntityQueryEnumerator<AlmayerComponent, ShuttleComponent>();
        while (query.MoveNext(out var uid, out _, out var shuttle))
        {
            if (TryComp<MCRuleCrashLandingComponent>(entity, out var landing))
            {
                landing.Shuttle = uid;
            }

            _shuttle.FTLToCoordinates(
                uid,
                shuttle,
                coordinates,
                Angle.Zero,
                hyperspaceTime: (float) entity.Comp.Time.TotalSeconds);
        }
    }

    private void OnLanded(Entity<AlmayerComponent> shuttle, ref FTLCompletedEvent args)
    {
        foreach (var rule in GameTicker.GetActiveGameRules())
        {
            if (!TryComp<MCRuleCrashLandingComponent>(rule, out var landing) || landing.Shuttle != shuttle.Owner || landing.Landed)
                continue;

            landing.Landed = true;
            var landed = new MCCrashLandedEvent();
            RaiseLocalEvent(rule, ref landed);
        }
    }

    private List<Entity<MCShuttlePointComponent>> GetPoints(Entity<MCRuleCrashShuttleComponent> entity)
    {
        var list = new List<Entity<MCShuttlePointComponent>>();
        var query = EntityQueryEnumerator<MCShuttlePointComponent>();

        while (query.MoveNext(out var uid, out var component))
        {
            if (!_whitelist.IsWhitelistPassOrNull(entity.Comp.PointWhitelist, uid))
                continue;

            list.Add((uid, component));
        }

        return list;
    }
}
