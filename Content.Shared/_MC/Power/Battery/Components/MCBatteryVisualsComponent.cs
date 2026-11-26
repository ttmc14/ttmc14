using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared._MC.Power.Battery.Components;

/// <summary>
/// Configures the sprite layer and charge levels used by the battery indicator.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class MCBatteryVisualsComponent : Component
{
    [DataField]
    public string Prefix = "o";

    [DataField]
    public int Levels = 4;

    [DataField]
    public string Layer = "power";
}

[Serializable, NetSerializable]
public enum MCBatteryVisuals : byte
{
    Layer,
}
