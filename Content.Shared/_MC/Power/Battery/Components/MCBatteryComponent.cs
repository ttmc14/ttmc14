using Robust.Shared.GameStates;

namespace Content.Shared._MC.Power.Battery.Components;

/// <summary>
/// Stores the current charge and maximum capacity of an MC battery.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MCBatteryComponent : Component
{
    [DataField, AutoNetworkedField]
    public float Charge;

    [DataField, AutoNetworkedField]
    public float MaxCharge;
}
