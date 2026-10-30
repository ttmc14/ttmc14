using Robust.Shared.Serialization;

namespace Content.Shared._MC.Xeno.QueueJoin.Events.Net;

[Serializable, NetSerializable]
public sealed class MCXenoHiveQueueJoinOfferEvent : EntityEventArgs
{
    public NetEntity? TargetEntity;
    public string TargetEntityName = string.Empty;
    public TimeSpan Expires;
    public string HiveName = string.Empty;
    public string OfferType = string.Empty;
    public int QueuePosition;
}
