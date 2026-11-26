using Content.Shared._MC.Rules.Warfare;
using Content.Shared._MC.Xeno.Hive.Components;
using Content.Shared._MC.Xeno.Hive.Join;
using Content.Shared._MC.Xeno.Hive.Systems.Main;
using Content.Shared._MC.Xeno.QueueJoin;
using Content.Shared._RMC14.CCVar;
using Content.Shared._RMC14.Dialog;
using Content.Shared._RMC14.GameTicking;
using Content.Shared._RMC14.Rules;
using Content.Shared._RMC14.Xenonids.Hive;
using Content.Shared.Actions;
using Content.Shared.GameTicking;
using Content.Shared.GameTicking.Components;
using Content.Shared.Ghost;
using Content.Shared.Popups;
using Robust.Shared.Configuration;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Timing;

namespace Content.Shared._RMC14.Xenonids.JoinXeno;

public sealed class JoinXenoSystem : EntitySystem
{
    [Dependency] private readonly MCWarfareSystem _warfare = null!;
    [Dependency] private readonly MCSharedXenoHiveSystem _mcHive = null!;
    [Dependency] private readonly MCXenoHiveJoinSystem _mcJoin = null!;
    [Dependency] private readonly MCXenoHiveQueueJoinSystem _mcQueue = null!;
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly DialogSystem _dialog = default!;
    [Dependency] private readonly SharedXenoHiveSystem _hive = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly SharedRMCGameTickerSystem _rmcGameTicker = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedGameTicker _gameTicker = default!;
    [Dependency] private readonly IConfigurationManager _config = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    public int ClientBurrowedLarva { get; private set; }

    private TimeSpan _burrowedLarvaDeathTime;
    private TimeSpan _burrowedLarvaDeathIgnoreTime;

    public override void Initialize()
    {
        SubscribeLocalEvent<RoundRestartCleanupEvent>(OnRoundRestartCleanup);

        SubscribeLocalEvent<JoinXenoComponent, MapInitEvent>(OnJoinXenoMapInit);
        SubscribeLocalEvent<JoinXenoComponent, JoinXenoActionEvent>(OnJoinXenoAction);
        SubscribeLocalEvent<JoinXenoComponent, JoinXenoBurrowedLarvaEvent>(OnJoinXenoBurrowedLarva);

        if (_net.IsClient)
        {
            SubscribeNetworkEvent<BurrowedLarvaStatusEvent>(OnBurrowedLarvaStatus);
        }
        else
        {
            SubscribeLocalEvent<RMCPlayerJoinedLobbyEvent>(OnPlayerJoinedLobby);
            SubscribeLocalEvent<BurrowedLarvaChangedEvent>(OnBurrowedLarvaChanged);
            SubscribeNetworkEvent<JoinBurrowedLarvaRequest>(OnJoinBurrowedLarva);
            SubscribeNetworkEvent<BurrowedLarvaStatusRequest>(OnBurrowedLarvaStatusRequest);
        }

        Subs.CVar(_config, RMCCVars.RMCLateJoinsBurrowedLarvaDeathTime, v => _burrowedLarvaDeathTime = TimeSpan.FromMinutes(v), true);
        Subs.CVar(_config, RMCCVars.RMCLateJoinsBurrowedLarvaDeathTimeIgnoreBeforeMinutes, v => _burrowedLarvaDeathIgnoreTime = TimeSpan.FromMinutes(v), true);
    }

    private void OnRoundRestartCleanup(RoundRestartCleanupEvent ev)
    {
        ClientBurrowedLarva = 0;
        SendLarvaStatus(null);
    }

    private void OnJoinXenoMapInit(Entity<JoinXenoComponent> ent, ref MapInitEvent args)
    {
        // _actions.AddAction(ent, ref ent.Comp.Action, ent.Comp.ActionId); // mc-clear-ghost
    }

    private void OnJoinXenoAction(Entity<JoinXenoComponent> ent, ref JoinXenoActionEvent args)
    {
        if (_net.IsClient)
            return;

        var user = args.Performer;
        if (!CanJoinXeno(user))
            return;

        var options = new List<DialogOption>();
        var hives = EntityQueryEnumerator<HiveComponent>();
        while (hives.MoveNext(out var hiveId, out var hive))
        {
            if (hive.BurrowedLarva <= 0)
                continue;

            options.Add(new DialogOption("Burrowed Larva", new JoinXenoBurrowedLarvaEvent(GetNetEntity(hiveId))));
        }

        _dialog.OpenOptions(ent, "Join as Xeno", options, "Available Xenonids");
    }

    public bool CanJoinXeno(EntityUid user)
    {
        if (!TryComp<GhostComponent>(user, out var ghostComp))
            return false;

        if (HasComp<JoinXenoCooldownIgnoreComponent>(user))
            return true;

        // If the game has been going on longer than the death ignore time, then check how long since the ghost has died
        if (_gameTicker.RoundDuration() > _burrowedLarvaDeathIgnoreTime)
        {
            var timeSinceDeath = _timing.CurTime.Subtract(ghostComp.TimeOfDeath);

            if (timeSinceDeath < _burrowedLarvaDeathTime)
            {
                var msg = Loc.GetString("rmc-xeno-ui-burrowed-need-time", ("seconds", _burrowedLarvaDeathTime.TotalSeconds - (int)timeSinceDeath.TotalSeconds));
                _popup.PopupEntity(msg, user, user, PopupType.MediumCaution);
                return false;
            }
        }

        return true;
    }

    private void OnJoinXenoBurrowedLarva(Entity<JoinXenoComponent> ent, ref JoinXenoBurrowedLarvaEvent args)
    {
        if (!TryGetEntity(args.Hive, out var hive) ||
            !TryComp(hive, out HiveComponent? hiveComp) ||
            !TryComp(ent, out ActorComponent? actor))
        {
            return;
        }

        _hive.JoinBurrowedLarva((hive.Value, hiveComp), actor.PlayerSession);
    }

    private void OnBurrowedLarvaStatus(BurrowedLarvaStatusEvent ev)
    {
        ClientBurrowedLarva = ev.Larva;

        if (_net.IsServer)
            return;

        var changedEv = new BurrowedLarvaChangedEvent(ev.Larva);
        RaiseLocalEvent(ref changedEv);
    }

    private void OnPlayerJoinedLobby(ref RMCPlayerJoinedLobbyEvent ev)
    {
        SendLarvaStatus(ev.Player);
    }

    private void OnBurrowedLarvaChanged(ref BurrowedLarvaChangedEvent ev)
    {
        SendLarvaStatus(null);
    }

    private void OnJoinBurrowedLarva(JoinBurrowedLarvaRequest msg, EntitySessionEventArgs args)
    {
        if (!_rmcGameTicker.PlayerGameStatuses.TryGetValue(args.SenderSession.UserId, out var status) ||
            status == PlayerGameStatus.JoinedGame)
        {
            return;
        }

        if (_warfare.ActiveRule is not null)
        {
            if (GetWarfareHive() is { } hive && _mcQueue.GetAvailableLarva(hive) > 0 &&
                _mcJoin.JoinBurrowedLarva(args.SenderSession, hive))
            {
                _rmcGameTicker.PlayerJoinGame(args.SenderSession);
            }
            return;
        }

        var query = EntityQueryEnumerator<CMDistressSignalRuleComponent>();
        while (query.MoveNext(out var comp))
        {
            if (!TryComp(comp.Hive, out HiveComponent? hive) ||
                !_hive.JoinBurrowedLarva((comp.Hive, hive), args.SenderSession))
            {
                continue;
            }

            _rmcGameTicker.PlayerJoinGame(args.SenderSession);
            break;
        }
    }

    private void OnBurrowedLarvaStatusRequest(BurrowedLarvaStatusRequest msg, EntitySessionEventArgs args)
    {
        SendLarvaStatus(args.SenderSession);
    }

    private Entity<MCXenoHiveComponent>? GetWarfareHive()
    {
        if (_mcHive.DefaultHive is not { } uid || !TryComp<MCXenoHiveComponent>(uid, out var hive))
            return null;

        return (uid, hive);
    }

    private void SendLarvaStatus(ICommonSession? to)
    {
        if (_net.IsClient)
            return;

        if (_warfare.ActiveRule is not null)
        {
            var available = GetWarfareHive() is { } hive ? _mcQueue.GetAvailableLarva(hive) : 0;
            SendLarvaStatus(to, available);
            return;
        }

        var query = EntityQueryEnumerator<ActiveGameRuleComponent, CMDistressSignalRuleComponent, GameRuleComponent>();
        while (query.MoveNext(out _, out var comp, out _))
        {
            if (!TryComp(comp.Hive, out HiveComponent? hive))
                continue;

            SendLarvaStatus(to, hive.BurrowedLarva);
            return;
        }
    }

    private void SendLarvaStatus(ICommonSession? to, int available)
    {
        var status = new BurrowedLarvaStatusEvent(available);
        if (to is not null)
        {
            RaiseNetworkEvent(status, to);
            return;
        }

        var lobby = Filter.Empty().AddWhere(session =>
            _rmcGameTicker.PlayerGameStatuses.GetValueOrDefault(session.UserId) != PlayerGameStatus.JoinedGame);
        RaiseNetworkEvent(status, lobby);
    }

    public void RequestBurrowedLarvaStatus()
    {
        if (_net.IsServer)
            return;

        var ev = new BurrowedLarvaStatusRequest();
        RaiseNetworkEvent(ev);
    }

    public void ClientJoinLarva()
    {
        if (_net.IsServer)
            return;

        var ev = new JoinBurrowedLarvaRequest();
        RaiseNetworkEvent(ev);
    }
}
