using Content.Shared._MC.Xeno.Hive.Components;

namespace Content.Shared._MC.Xeno.Hive.Events;

[ByRefEvent]
public record struct MCXenoHiveChangedEvent(Entity<MCXenoHiveComponent>? Hive, EntityUid? OldHive);
