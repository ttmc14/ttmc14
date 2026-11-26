namespace Content.Shared._MC.Weapon.Laser.Events;

/// <summary>
/// Raised on the gun's parent when a laser cell changes charge and can accept more energy.
/// </summary>
[ByRefEvent]
public record struct MCLaserCellChargeChangedEvent(EntityUid Gun, EntityUid Cell)
{
    public bool Recharged;
}
