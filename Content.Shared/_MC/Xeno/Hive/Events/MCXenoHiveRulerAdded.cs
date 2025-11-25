namespace Content.Shared._MC.Xeno.Hive.Events;

[ByRefEvent]
public readonly record struct MCXenoHiveRulerAdded(EntityUid RulerUid, EntityUid HiveUid);
