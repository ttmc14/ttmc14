using Robust.Shared.Serialization;

namespace Content.Shared._MC.Xeno.QueueJoin.Events;

[Serializable, NetSerializable]
public sealed partial class MCXenoHiveQueueJoinEvent(NetEntity hiveUid) : EntityEventArgs
{
    public readonly NetEntity HiveUid = hiveUid;
}
