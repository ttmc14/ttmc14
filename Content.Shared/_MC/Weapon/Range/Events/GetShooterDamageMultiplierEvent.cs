namespace Content.Shared._MC.Weapon.Range.Events;

/// <summary>Raised on the shooter to collect damage multipliers for a ranged shot.</summary>
[ByRefEvent]
public record struct GetShooterDamageMultiplierEvent(float Multiplier);
