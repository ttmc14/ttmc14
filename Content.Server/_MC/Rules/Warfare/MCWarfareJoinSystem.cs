using Content.Server._MC.Rules.Common;
using Content.Server._MC.Xeno.Hive;
using Content.Server.GameTicking;
using Content.Shared._MC.Rules.Common;
using Content.Shared._MC.Rules.Warfare;
using Content.Shared._MC.Xeno.Hive.Components;
using Content.Shared._MC.Xeno.Hive.Join;
using Content.Shared._MC.Xeno.QueueJoin;
using Content.Shared._MC.Xeno.QueueJoin.Events;
using Content.Shared.GameTicking;
using Content.Shared.GameTicking.Components;
using Robust.Shared.Player;

namespace Content.Server._MC.Rules.Warfare;

public sealed class MCWarfareJoinSystem : EntitySystem
{
    [Dependency] private readonly MCRuleXenoMarinesSpawnSystem _spawn = null!;
    [Dependency] private readonly GameTicker _ticker = null!;
    [Dependency] private readonly MCXenoHiveSystem _hive = null!;
    [Dependency] private readonly MCXenoHiveJoinSystem _join = null!;
    [Dependency] private readonly MCXenoHiveQueueJoinSystem _queue = null!;

    public override void Initialize()
    {
        SubscribeNetworkEvent<MCRequestLobbyLarvaStatusEvent>(OnStatusRequest);
        SubscribeNetworkEvent<MCJoinLobbyLarvaEvent>(OnJoinRequest);
        SubscribeLocalEvent<MCBeforeJoinBurrowedLarvaEvent>(OnBeforeJoinLarva);
        SubscribeLocalEvent<MCAvailableLarvaChangedEvent>(OnAvailableLarvaChanged);
    }

    private void OnStatusRequest(MCRequestLobbyLarvaStatusEvent message, EntitySessionEventArgs args)
    {
        if (!IsInLobby(args.SenderSession))
            return;

        int? available = null;
        if (_ticker.IsGameRuleActive<MCRuleWarfareComponent>())
        {
            available = CanLateJoin() && TryGetHive(out var hive) ? _queue.GetAvailableLarva(hive) : 0;
        }

        RaiseNetworkEvent(new MCLobbyLarvaStatusEvent(available), args.SenderSession);
    }

    private void OnJoinRequest(MCJoinLobbyLarvaEvent message, EntitySessionEventArgs args)
    {
        var session = args.SenderSession;
        if (!IsInLobby(session) || !CanLateJoin() || !_ticker.IsGameRuleActive<MCRuleWarfareComponent>())
            return;

        if (TryGetHive(out var hive) && _queue.GetAvailableLarva(hive) > 0 && _join.JoinBurrowedLarva(session, hive))
        {
            _ticker.PlayerJoinGame(session);
        }
    }

    private bool IsInLobby(ICommonSession session) =>
        _ticker.PlayerGameStatuses.TryGetValue(session.UserId, out var status) &&
        status is PlayerGameStatus.NotReadyToPlay or PlayerGameStatus.ReadyToPlay;

    private bool CanLateJoin() => _ticker.RunLevel == GameRunLevel.InRound && !_ticker.DisallowLateJoin;

    private void OnBeforeJoinLarva(ref MCBeforeJoinBurrowedLarvaEvent args)
    {
        var query = EntityQueryEnumerator<MCRuleWarfareComponent, ActiveGameRuleComponent>();
        if (!query.MoveNext(out var uid, out _, out _))
            return;

        var job = CompOrNull<MCRuleXenoMarinesSpawnComponent>(uid)?.XenoOverflow.JobId ??
            MCRuleXenoMarinesSpawnComponent.DefaultOverflowJob;
        if (!_spawn.IsAllowed(args.Player, job))
        {
            args.Cancelled = true;
        }
    }

    private bool TryGetHive(out Entity<MCXenoHiveComponent> hive)
    {
        hive = default;
        if (_hive.DefaultHive is not { } uid || !TryComp<MCXenoHiveComponent>(uid, out var component))
            return false;

        hive = (uid, component);
        return true;
    }

    private void OnAvailableLarvaChanged(ref MCAvailableLarvaChangedEvent args)
    {
        if (!_ticker.IsGameRuleActive<MCRuleWarfareComponent>() || args.Hive != _hive.DefaultHive)
            return;

        var lobby = Filter.Empty().AddWhere(IsInLobby);
        RaiseNetworkEvent(new MCLobbyLarvaStatusEvent(CanLateJoin() ? args.Available : 0), lobby);
    }
}
