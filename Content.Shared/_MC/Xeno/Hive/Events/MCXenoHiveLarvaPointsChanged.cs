namespace Content.Shared._MC.Xeno.Hive.Events;

[ByRefEvent]
public readonly record struct MCXenoHiveLarvaPointsChanged(int Previous, int Current);
