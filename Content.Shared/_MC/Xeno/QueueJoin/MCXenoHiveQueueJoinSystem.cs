using System.Diagnostics.CodeAnalysis;
using Content.Shared._MC._Config;
using Content.Shared._MC.Utilities;
using Content.Shared._MC.Xeno.Hive.Components;
using Content.Shared._MC.Xeno.Hive.Join;
using Content.Shared._MC.Xeno.Hive.Systems.Main;
using Content.Shared._MC.Xeno.QueueJoin.Components;
using Content.Shared._MC.Xeno.QueueJoin.Events;
using Content.Shared._MC.Xeno.QueueJoin.Events.Net;
using Content.Shared._RMC14.Dialog;
using Content.Shared._RMC14.Xenonids.Hive;
using Content.Shared.Actions;
using Content.Shared.Ghost;
using Content.Shared.Popups;
using Robust.Shared.Configuration;
using Robust.Shared.GameStates;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Timing;
using Robust.Shared.Utility;

namespace Content.Shared._MC.Xeno.QueueJoin;

[RegisterComponent, NetworkedComponent]
public sealed partial class MCXenoHiveQueueJoinSingletonComponent : Component
{
    // NOTE: Key is hive entity.
    [ViewVariables]
    public readonly Dictionary<EntityUid, LinkedList<NetUserId>> Queue = new();

    [ViewVariables]
    public readonly Dictionary<NetUserId,  MCXenoHiveQueueJoinOffer> Offers = new();

    [ViewVariables]
    public readonly HashSet<EntityUid> Reserved = new();

    [ViewVariables]
    public readonly Dictionary<EntityUid, int> PendingBurrowedCount = new();
}

public sealed partial class MCXenoHiveQueueJoinSystem : MCEntitySystemSingleton<MCXenoHiveQueueJoinSingletonComponent>
{
    [Dependency] private readonly IConfigurationManager _configuration = null!;
    [Dependency] private readonly ISharedPlayerManager _player = null!;
    [Dependency] private readonly IGameTiming _timing = null!;

    [Dependency] private readonly SharedActionsSystem _actions = null!;
    [Dependency] private readonly SharedPopupSystem _popup = null!;
    [Dependency] private readonly DialogSystem _dialog = null!;

    [Dependency] private readonly MCSharedXenoHiveSystem _hive = null!;
    [Dependency] private readonly MCXenoHiveJoinSystem _hiveJoin = null!;

    private TimeSpan _joinDelay = TimeSpan.Zero;
    private TimeSpan _offerTimeout = TimeSpan.Zero;

    private Dictionary<EntityUid, LinkedList<NetUserId>> Queue => Inst.Comp.Queue;
    private Dictionary<NetUserId, MCXenoHiveQueueJoinOffer> Offers => Inst.Comp.Offers;
    private HashSet<EntityUid> Reserved => Inst.Comp.Reserved;
    private Dictionary<EntityUid, int> PendingBurrowedCount => Inst.Comp.PendingBurrowedCount;

    public override void Initialize()
    {
        _configuration.OnValueChanged(MCConfigVars.XenoQueueJoinWaitSeconds, e => _joinDelay = TimeSpan.FromSeconds(e));
        _configuration.OnValueChanged(MCConfigVars.XenoQueueJoinOfferTimeoutSeconds, e => _offerTimeout = TimeSpan.FromSeconds(e));

        SubscribeLocalEvent<MCXenoHiveQueueJoinComponent, ComponentStartup>(OnJoinStartup);
        SubscribeLocalEvent<MCXenoHiveQueueJoinComponent, ComponentRemove>(OnJoinRemove);

        SubscribeLocalEvent<MCXenoHiveQueueJoinComponent, MCXenoHiveQueueJoinActionEvent>(OnJoinAction);

        SubscribeLocalEvent<MCXenoHiveQueueJoinComponent, MCXenoHiveQueueJoinEvent>(OnJoinQueue);

        SubscribeNetworkEvent<MCXenoHiveQueueJoinOfferAcceptEvent>(OnOfferAccept);
        SubscribeNetworkEvent<MCXenoHiveQueueJoinOfferRejectEvent>(OnOfferReject);
    }

    private void OnJoinStartup(Entity<MCXenoHiveQueueJoinComponent> entity, ref ComponentStartup args)
    {
        _actions.AddAction(entity, ref entity.Comp.Action, entity.Comp.ActionId);
    }

    private void OnJoinRemove(Entity<MCXenoHiveQueueJoinComponent> entity, ref ComponentRemove args)
    {
        _actions.RemoveAction(entity.Comp.Action);
    }

    private void OnJoinQueue(Entity<MCXenoHiveQueueJoinComponent> entity, ref MCXenoHiveQueueJoinEvent args)
    {
        if (!TryComp<GhostComponent>(entity, out var ghostComponent) ||
            !TryComp<ActorComponent>(entity, out var actorComponent))
            return;

        var hiveUid = GetEntity(args.HiveUid);
        if (!TryComp<MCXenoHiveComponent>(hiveUid, out _))
            return;

        var deathDeltaTime = _timing.CurTime - ghostComponent.TimeOfDeath;
        if (deathDeltaTime < _joinDelay)
        {
            var timeLeft = _joinDelay - deathDeltaTime;
            _popup.PopupEntity($"You died too recently, and will be added to the queue in {timeLeft.TotalSeconds:F0} seconds.", entity, entity);
            return;
        }

        var userId = actorComponent.PlayerSession.UserId;

        var queue = Queue.GetOrNew(hiveUid);
        if (queue.Contains(userId))
        {
            Queue.GetOrNew(hiveUid).Remove(userId);
            _popup.PopupEntity("You have been removed from the queue.", entity, entity);
            return;
        }

        Queue.GetOrNew(hiveUid).AddLast(userId);

        var position = queue.PositionOf(userId);
        _popup.PopupEntity($"You have been added to the queue at position {position}.", entity, entity);
    }

    private void SendOffer(ICommonSession session, Entity<MCXenoHiveComponent> hive, EntityUid? targetUid, int position)
    {
        RevokePendingOffer(session);

        var expires = _timing.CurTime + _offerTimeout;

        Offers[session.UserId] = new MCXenoHiveQueueJoinOffer
        {
            TargetUid = null,
            HiveEntity = hive,
            Expires = expires,
        };

        RaiseNetworkEvent(new MCXenoHiveQueueJoinOfferEvent
        {
            TargetEntity = targetUid.HasValue ? GetNetEntity(targetUid.Value) : null,
            TargetEntityName = targetUid.HasValue ? Name(targetUid.Value) : "Burrowed Larva",
            Expires = expires,
            HiveName = Name(hive),
            QueuePosition = position,
        },
        session);
    }

    private void TryOffer(Entity<MCXenoHiveComponent> hive)
    {
        if (!HasAnyQueued(hive.Owner))
            return;

        var available = hive.Comp.BurrowedLarva - PendingBurrowedCount.GetValueOrDefault(hive, 0);
        for (var i = 0; i < available; i++)
        {
            if (!HasAnyQueued(hive.Owner))
                break;

            if (!TryPopNextSessionForOffer(hive, out var session, out var pos))
                break;

            PendingBurrowedCount[hive] = PendingBurrowedCount.GetValueOrDefault(hive, 0) + 1;
            SendOffer(session, hive, null, pos);
        }
    }

    private void RevokePendingOffer(ICommonSession session)
    {
        if (!Offers.TryGetValue(session.UserId, out var existingOffer))
            return;

        if (existingOffer.TargetUid is { } existingUid)
        {
            Reserved.Remove(existingUid);
            return;
        }

        DecrementPendingBurrowed(existingOffer.HiveEntity);
    }

    private void DecrementPendingBurrowed(Entity<MCXenoHiveComponent> hive)
    {
        var current = PendingBurrowedCount.GetValueOrDefault(hive, 0);
        if (current <= 1)
        {
            PendingBurrowedCount.Remove(hive);
            return;
        }

        PendingBurrowedCount[hive] = current - 1;
    }

    private bool HasAnyQueued(EntityUid hive)
    {
        return Queue.TryGetValue(hive, out var list) && list.Count > 0;
    }

    private bool TryPopNextSessionForOffer(
        Entity<MCXenoHiveComponent> hive,
        [NotNullWhen(true)] out ICommonSession? nextSession,
        out int position)
    {
        nextSession = null;
        position = 0;

        Queue.TryGetValue(hive, out var queue);

        return queue is not null && TryPopFromList(queue, 0, out nextSession, out position);
    }

    private bool TryPopFromList(
        LinkedList<NetUserId> list,
        int offset,
        [NotNullWhen(true)] out ICommonSession? session,
        out int absolutePosition)
    {
        session = null;
        absolutePosition = 0;

        var indexInList = 0;

        var node = list.First;
        while (node is not null)
        {
            var next = node.Next;
            var netId = node.Value;

            indexInList++;

            if (Offers.ContainsKey(netId))
            {
                node = next;
                continue;
            }

            if (!_player.TryGetSessionById(netId, out var commonSession) || commonSession.AttachedEntity == null)
            {
                // if (!_disconnectExpiresAt.ContainsKey(netId))
                //    _disconnectExpiresAt[netId] = _gameTiming.CurTime + _disconnectGracePeriod;
                node = next;
                continue;
            }

            if (!HasComp<GhostComponent>(commonSession.AttachedEntity.Value))
            {
                list.Remove(node);
                node = next;
                continue;
            }

            absolutePosition = offset + indexInList;
            session = commonSession;
            return true;
        }

        return false;
    }
}
