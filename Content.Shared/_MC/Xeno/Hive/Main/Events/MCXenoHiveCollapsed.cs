using Content.Shared._MC.Xeno.Hive.Components;

namespace Content.Shared._MC.Xeno.Hive.Events;

[ByRefEvent]
public readonly record struct MCXenoHiveCollapsed(EntityUid HiveUid, MCXenoHiveCollapseType Type);
