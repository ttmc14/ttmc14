using JetBrains.Annotations;

namespace Content.Shared._MC.Power.Battery.Events;

[ByRefEvent, PublicAPI]
public readonly struct MCBatteryChargeChangedEvent(float previousCharge, float charge, float maxCharge)
{
    public readonly float PreviousCharge = previousCharge;
    public readonly float Charge = charge;
    public readonly float MaxCharge = maxCharge;

    public float Ration => float.Clamp(Charge / MaxCharge, 0f, 1f);
}
