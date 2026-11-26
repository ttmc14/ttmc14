using Content.Server._MC.Nuke.Bomb;
using Content.Server.Chat.Managers;
using Content.Server.GameTicking;
using Content.Server.Shuttles.Components;
using Content.Server.Shuttles.Systems;
using Content.Shared._MC.Living;
using Content.Shared._MC.Rules.Common;
using Content.Shared._MC.Shuttle;
using Content.Shared._MC.Shuttle.Console;
using Content.Shared._MC.Shuttle.Console.UI;
using Content.Shared._MC.Shuttle.Events;
using Content.Shared._RMC14.Marines;
using Content.Shared._RMC14.Marines.Announce;
using Content.Shared._RMC14.Rules;
using Content.Shared._RMC14.Thunderdome;
using Content.Shared._RMC14.Xenonids;
using Content.Shared.Coordinates;
using Content.Shared.Shuttles.Components;
using Content.Shared.UserInterface;
using Robust.Shared.Timing;

namespace Content.Server._MC.Shuttles;

public sealed class MCShuttleEvacuationSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = null!;
    [Dependency] private readonly IChatManager _chat = null!;
    [Dependency] private readonly GameTicker _ticker = null!;
    [Dependency] private readonly MCNukeSystem _nuke = null!;
    [Dependency] private readonly MCLivingSystem _living = null!;
    [Dependency] private readonly SharedMarineAnnounceSystem _announce = null!;
    [Dependency] private readonly SharedMapSystem _maps = null!;
    [Dependency] private readonly ShuttleSystem _shuttles = null!;
    [Dependency] private readonly SharedUserInterfaceSystem _ui = null!;

    private static readonly TimeSpan RefuelTime = TimeSpan.FromMinutes(10);
    private TimeSpan _nextRefresh;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<MCShuttleConsoleComponent, ActivatableUIOpenAttemptEvent>(OnOpenAttempt);
        SubscribeLocalEvent<MCShuttleConsoleComponent, MCShuttleConsoleEvacuateBuiMessage>(OnRequest);
    }

    private void OnOpenAttempt(Entity<MCShuttleConsoleComponent> console, ref ActivatableUIOpenAttemptEvent args)
    {
        if (HasComp<XenoComponent>(args.User))
        {
            args.Cancel();
        }
    }

    private void OnRequest(Entity<MCShuttleConsoleComponent> console, ref MCShuttleConsoleEvacuateBuiMessage args)
    {
        if (HasComp<XenoComponent>(args.Actor) || Transform(console).GridUid is not { } grid ||
            !TryComp<MCShuttleComponent>(grid, out var shuttle) || !CanDepart(grid) ||
            shuttle.Phase != MCEvacuationPhase.Idle || shuttle.Evacuated)
            return;

        if (_nuke.HasActiveCountdown())
        {
            Launch((grid, shuttle));
            return;
        }

        if (!args.Confirmed)
            return;

        shuttle.Phase = MCEvacuationPhase.Review;
        shuttle.NextTransition = _timing.CurTime + TimeSpan.FromSeconds(10);
        var marines = _living.Get<MarineComponent>(marine => !HasComp<ThunderdomeMapComponent>(Transform(marine).MapUid));
        _chat.SendAdminAlert(Loc.GetString("mc-evac-admin", ("user", ToPrettyString(args.Actor)),
            ("shuttle", grid.ToString()), ("count", marines)));
    }

    /// <summary>Resolves a pending early departure request before its deadline.</summary>
    public bool ResolveRequest(EntityUid grid, bool approve, bool silent)
    {
        if (!TryComp<MCShuttleComponent>(grid, out var shuttle) || shuttle.Phase != MCEvacuationPhase.Review ||
            shuttle.NextTransition <= _timing.CurTime)
            return false;

        if (approve)
        {
            Launch((grid, shuttle));
            return true;
        }

        shuttle.Phase = MCEvacuationPhase.Restarting;
        shuttle.NextTransition = _timing.CurTime + TimeSpan.FromSeconds(15);
        if (!silent)
        {
            Announce("mc-evac-denied");
        }

        return true;
    }

    private bool CanDepart(EntityUid grid)
    {
        return _ticker.IsGameRuleActive<MCRuleCrashShuttleComponent>() && _ticker.RoundDuration() >= RefuelTime &&
            HasComp<RMCPlanetComponent>(Transform(grid).MapUid) && HasComp<ShuttleComponent>(grid) &&
            !HasComp<FTLComponent>(grid);
    }

    private void Launch(Entity<MCShuttleComponent> shuttle)
    {
        shuttle.Comp.Phase = MCEvacuationPhase.Idle;
        if (!CanDepart(shuttle) || !TryComp<ShuttleComponent>(shuttle, out var drive))
            return;

        var destination = _maps.CreateMap();
        _shuttles.FTLToCoordinates(shuttle, drive, destination.ToCoordinates(), Angle.Zero, 5f, 120f);
        shuttle.Comp.Phase = MCEvacuationPhase.Departing;
        shuttle.Comp.NextTransition = _timing.CurTime + TimeSpan.FromSeconds(15);
        Announce("mc-evac-departure");
    }

    private void Announce(string message)
    {
        _announce.AnnounceHighCommand(Loc.GetString(message), Loc.GetString("mc-evac-title"));
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);
        var shuttles = EntityQueryEnumerator<MCShuttleComponent>();
        while (shuttles.MoveNext(out var grid, out var shuttle))
        {
            if (shuttle.Phase is MCEvacuationPhase.Idle or MCEvacuationPhase.Complete || shuttle.NextTransition > _timing.CurTime)
                continue;

            switch (shuttle.Phase)
            {
                case MCEvacuationPhase.Review:
                    Launch((grid, shuttle));
                    break;
                case MCEvacuationPhase.Restarting:
                    shuttle.Phase = MCEvacuationPhase.Idle;
                    break;
                case MCEvacuationPhase.Departing:
                    shuttle.Evacuated = true;
                    Dirty(grid, shuttle);
                    shuttle.Phase = MCEvacuationPhase.InTransit;
                    shuttle.NextTransition += TimeSpan.FromSeconds(105);
                    RaiseLocalEvent(new MCShuttleEvacuationEvent());
                    break;
                case MCEvacuationPhase.InTransit:
                    shuttle.Phase = MCEvacuationPhase.Complete;
                    RaiseLocalEvent(new MCShuttleEvacuationEvent(true));
                    break;
            }
        }

        if (_nextRefresh > _timing.CurTime)
            return;

        _nextRefresh = _timing.CurTime + TimeSpan.FromMilliseconds(250);
        var consoles = EntityQueryEnumerator<MCShuttleConsoleComponent>();
        while (consoles.MoveNext(out var console, out _))
        {
            if (!_ui.IsUiOpen(console, MCShuttleConsoleUi.Key))
                continue;

            var state = GetState(Transform(console).GridUid);
            if (_ui.TryGetUiState<MCShuttleConsoleState>(console, MCShuttleConsoleUi.Key, out var previous) &&
                previous.Status == state.Status && previous.Seconds == state.Seconds)
                continue;

            _ui.SetUiState(console, MCShuttleConsoleUi.Key, state);
        }
    }

    private MCShuttleConsoleState GetState(EntityUid? grid)
    {
        if (grid is not { } uid || !TryComp<MCShuttleComponent>(uid, out var shuttle) ||
            !HasComp<ShuttleComponent>(uid) || !_ticker.IsGameRuleActive<MCRuleCrashShuttleComponent>())
            return new("offline");

        if (shuttle.Phase != MCEvacuationPhase.Idle)
            return new(shuttle.Phase.ToString().ToLowerInvariant(), SecondsLeft(shuttle.NextTransition - _timing.CurTime));

        if (shuttle.Evacuated)
            return new("complete");

        if (_ticker.RoundDuration() < RefuelTime)
            return new("refuel", SecondsLeft(RefuelTime - _ticker.RoundDuration()));

        if (!CanDepart(uid))
            return new("unavailable");

        return new(_nuke.HasActiveCountdown() ? "ready" : "early");
    }

    private static int SecondsLeft(TimeSpan remaining) => Math.Max(0, (int) Math.Ceiling(remaining.TotalSeconds));
}
