using Robust.Shared.GameStates;

namespace Content.Shared._MC.Electrical.PowerCell.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(fieldDeltas: true)]
public sealed partial class MCPowerCellComponent : Component
{
    [DataField, AutoNetworkedField]
    public float MaxCharge;

    [DataField, AutoNetworkedField]
    public float Charge;
}
