using Content.Shared._MC.Engineering.Beacon.Components;

namespace Content.Shared._MC.Engineering.Beacon.Events;

[ByRefEvent]
public record struct MCBeaconActiveChangedEvent(Entity<MCBeaconComponent> Entity, bool Added);
