using Content.Shared._MC.Xeno.Hive.Components;

namespace Content.Shared._MC.Xeno.QueueJoin;

public readonly struct MCXenoHiveQueueJoinOffer
{
    public EntityUid? TargetUid { get; init; }
    public Entity<MCXenoHiveComponent> HiveEntity { get; init; }
    public TimeSpan Expires { get; init; }
}
