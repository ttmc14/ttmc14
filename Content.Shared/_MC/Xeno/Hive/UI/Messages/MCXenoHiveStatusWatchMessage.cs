using Robust.Shared.Serialization;

namespace Content.Shared._MC.Xeno.Hive.UI.Messages;

[Serializable, NetSerializable]
public sealed class MCXenoHiveStatusWatchMessage(NetEntity entity) : BoundUserInterfaceMessage
{
    public readonly NetEntity TagetEntity = entity;
}
