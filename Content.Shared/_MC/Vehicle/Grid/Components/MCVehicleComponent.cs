using Robust.Shared.GameStates;
using Robust.Shared.Utility;

namespace Content.Shared._MC.Vehicle.Grid.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(fieldDeltas: true)]
public sealed partial class MCVehicleComponent : Component
{
    [DataField, AutoNetworkedField]
    public ResPath Path = new("/Maps/_MC/Vehicles/som_tank.yml");

    [DataField, AutoNetworkedField]
    public EntityUid? GridUid;
}
