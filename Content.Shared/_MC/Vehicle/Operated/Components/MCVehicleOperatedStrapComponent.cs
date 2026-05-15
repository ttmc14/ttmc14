using Robust.Shared.GameStates;

namespace Content.Shared._MC.Vehicle.Operated.Components;

[Access(typeof(MCVehicleOperatedSystem))]
[RegisterComponent, NetworkedComponent]
public sealed partial class MCVehicleOperatedStrapComponent : Component;

