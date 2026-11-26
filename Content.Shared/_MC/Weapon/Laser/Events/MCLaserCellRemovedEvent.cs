namespace Content.Shared._MC.Weapon.Laser.Events;

/// <summary>
/// Raised on the gun's parent when an item leaves its cell slot.
/// </summary>
[ByRefEvent]
public readonly record struct MCLaserCellRemovedEvent(EntityUid Gun);
