namespace Content.Shared._MC.Weapon.Range.Events;

/// <summary>Raised on the shooter to collect additional spread for a ranged shot.</summary>
[ByRefEvent]
public record struct GetShooterSpreadEvent(Angle AdditionalSpread);
