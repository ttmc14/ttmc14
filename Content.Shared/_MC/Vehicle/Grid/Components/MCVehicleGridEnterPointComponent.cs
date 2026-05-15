using Robust.Shared.GameStates;

namespace Content.Shared._MC.Vehicle.Grid.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(fieldDeltas: true)]
public sealed partial class MCVehicleGridEnterPointComponent : Component
{
    [DataField, AutoNetworkedField]
    public Direction Direction;
}
