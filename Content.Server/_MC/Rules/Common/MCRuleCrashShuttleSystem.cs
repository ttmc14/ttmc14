using Content.Server._MC.Rules.Crash;
using Content.Server.GameTicking;
using Content.Server.GameTicking.Rules;
using Content.Server.Shuttles.Components;
using Content.Server.Shuttles.Systems;
using Content.Shared._MC.Rules.Common;
using Content.Shared._MC.Rules.Crash;
using Content.Shared._RMC14.Marines;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Server._MC.Rules.Common;

public sealed class MCRuleCrashShuttleSystem : GameRuleSystem<MCRuleCrashShuttleComponent>
{
    [Dependency] private readonly IPrototypeManager _prototype = null!;
    [Dependency] private readonly IRobustRandom _random = null!;

    [Dependency] private readonly ShuttleSystem _shuttle = null!;

    [Dependency] private readonly MCRelayedSystem _relayed = null!;
    [Dependency] private readonly MCCrashMapVoteSystem _mapVote = null!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<LoadingMapsEvent>(OnMapLoading);
        SubscribeLocalEvent<RulePlayerSpawningEvent>(OnRulePlayerSpawning);
    }

    private void OnMapLoading(LoadingMapsEvent ev)
    {
        if (!GameTicker.IsGameRuleAdded<MCRuleCrashShuttleComponent>())
            return;

        ev.Maps.Clear();
        ev.Maps.Add(_prototype.Index(_mapVote.SelectedMap));

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
            Crash(entity.Comp.Time);
        });
    }

    private void Crash(TimeSpan time)
    {
        var crashPoints = GetPoints();
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
            _shuttle.FTLToCoordinates(
                uid,
                shuttle,
                coordinates,
                Angle.Zero,
                hyperspaceTime: (float) time.TotalSeconds);
        }
    }

    private List<Entity<MCCrashPointComponent>> GetPoints()
    {
        var list = new List<Entity<MCCrashPointComponent>>();
        var query = AllEntityQuery<MCCrashPointComponent>();

        while (query.MoveNext(out var uid, out var component))
        {
            list.Add((uid, component));
        }

        return list;
    }
}
