using Content.Shared._MC.Xeno.Hive.Components;

namespace Content.Shared._MC.Xeno.Hive.Main.Events;

[ByRefEvent]
public record struct MCXenoHiveBurrowedLarvaChangedEvent(Entity<MCXenoHiveComponent> HiveEntity, int PreviousCount, int Count);
