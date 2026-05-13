namespace Content.Shared._MC.Electrical.PowerCell.Events;

[ByRefEvent]
public readonly record struct MCChargeChangedEvent(float Charge, float MaxCharge);
