using Robust.Shared.GameStates;

namespace Content.Shared._MC.Marine.Equipment.ThermalCloak;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MCThermalCloakComponent : Component
{
    [DataField, AutoNetworkedField]
    public float Energy = 100f;

    [DataField, AutoNetworkedField]
    public float EnergyMax = 100f;
}
