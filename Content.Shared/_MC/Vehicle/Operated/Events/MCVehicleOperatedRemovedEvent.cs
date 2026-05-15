using Content.Shared._MC.Vehicle.Operated.Components;

namespace Content.Shared._MC.Vehicle.Operated.Events;

[ByRefEvent]
public readonly record struct MCVehicleOperatedRemovedEvent(Entity<MCVehicleOperatedComponent> Vehicle, EntityUid Operator);
