using System.Numerics;
using Robust.Shared.GameStates;

namespace Content.Shared._MC.Vehicle.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(fieldDeltas: true)]
public sealed partial class MCVehicleMapComponent : Component
{
    [DataField, AutoNetworkedField]
    public Vector2 Offset;
}
