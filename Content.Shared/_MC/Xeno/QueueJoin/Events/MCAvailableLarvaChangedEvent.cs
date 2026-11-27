namespace Content.Shared._MC.Xeno.QueueJoin.Events;

[ByRefEvent]
public readonly record struct MCAvailableLarvaChangedEvent(EntityUid Hive, int Available);
