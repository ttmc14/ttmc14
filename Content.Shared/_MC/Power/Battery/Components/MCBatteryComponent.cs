using Robust.Shared.GameStates;

namespace Content.Shared._MC.Power.Battery.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MCBatteryComponent : Component
{
    [DataField, AutoNetworkedField]
    public float Charge;

    [DataField, AutoNetworkedField]
    public float MaxCharge;
}
