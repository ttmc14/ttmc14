namespace Content.Shared._MC.Xeno.Abilities.Ravager.Rage.Events;

[ByRefEvent]
public record struct MCXenoRagePowerChangedEvent(float Power, float PreviousPower);
