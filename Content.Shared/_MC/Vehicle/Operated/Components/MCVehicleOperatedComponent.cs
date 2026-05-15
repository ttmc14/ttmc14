using Robust.Shared.GameStates;

namespace Content.Shared._MC.Vehicle.Operated.Components;

[Access(typeof(MCVehicleOperatedSystem))]
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MCVehicleOperatedComponent : Component
{
    [DataField, AutoNetworkedField]
    public EntityUid? Operator;
}
