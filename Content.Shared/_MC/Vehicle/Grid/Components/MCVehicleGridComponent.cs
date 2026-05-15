using Robust.Shared.GameStates;

namespace Content.Shared._MC.Vehicle.Grid.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(fieldDeltas: true)]
public sealed partial class MCVehicleGridComponent : Component
{
    [DataField, AutoNetworkedField]
    public EntityUid? OwnerUid;
}
