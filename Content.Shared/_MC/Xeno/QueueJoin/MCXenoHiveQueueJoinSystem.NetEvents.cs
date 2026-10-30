using Content.Shared._MC.Xeno.QueueJoin.Events.Net;

namespace Content.Shared._MC.Xeno.QueueJoin;

public sealed partial class MCXenoHiveQueueJoinSystem
{
    private void OnOfferAccept(MCXenoHiveQueueJoinOfferAcceptEvent message, EntitySessionEventArgs args)
    {
        var userId = args.SenderSession.UserId;

        if (!Offers.Remove(userId, out var offer))
            return;

        if (offer.TargetUid is { } targetUid)
        {

        }

        _hiveJoin.JoinBurrowedLarva(args.SenderSession, offer.HiveEntity);
    }

    private void OnOfferReject(MCXenoHiveQueueJoinOfferRejectEvent message, EntitySessionEventArgs args)
    {
        var userId = args.SenderSession.UserId;

        if (!Offers.Remove(userId, out var offer))
            return;

    }
}
