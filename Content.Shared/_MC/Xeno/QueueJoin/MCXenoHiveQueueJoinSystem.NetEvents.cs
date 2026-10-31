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

        TryRemoveFromQueue(userId, offer.HiveEntity);
        if (!_hiveJoin.JoinBurrowedLarva(args.SenderSession, offer.HiveEntity))
            return;

        // TODO: fuck....
    }

    private void OnOfferReject(MCXenoHiveQueueJoinOfferRejectEvent message, EntitySessionEventArgs args) =>
        ExpireOffer(args.SenderSession.UserId, true);
}
