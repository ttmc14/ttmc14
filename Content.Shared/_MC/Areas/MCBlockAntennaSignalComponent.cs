using Robust.Shared.GameStates;

namespace Content.Shared._MC.Areas;

/// <summary>
/// Blocks helmet supply beacons inside this area or aboard this grid.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class MCBlockAntennaSignalComponent : Component;
