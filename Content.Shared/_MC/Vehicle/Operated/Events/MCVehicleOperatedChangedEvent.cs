namespace Content.Shared._MC.Vehicle.Operated.Events;

[ByRefEvent]
public readonly record struct MCVehicleOperatedChangedEvent(EntityUid? NewOperator, EntityUid? OldOperator);
