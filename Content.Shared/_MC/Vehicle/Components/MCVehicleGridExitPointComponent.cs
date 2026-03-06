using Robust.Shared.GameStates;

namespace Content.Shared._MC.Vehicle.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(fieldDeltas: true)]
public sealed partial class MCVehicleGridExitPointComponent : Component
{
    [DataField, AutoNetworkedField]
    public Direction Direction;
}
