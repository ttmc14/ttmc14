using Robust.Shared.Serialization;

namespace Content.Shared._MC.Xeno.Hive.Join;

[Serializable, NetSerializable]
public sealed class MCRequestLobbyLarvaStatusEvent : EntityEventArgs;

[Serializable, NetSerializable]
public sealed class MCJoinLobbyLarvaEvent : EntityEventArgs;

[Serializable, NetSerializable]
public sealed class MCLobbyLarvaStatusEvent(int? available) : EntityEventArgs
{
    public int? Available { get; } = available;
}
