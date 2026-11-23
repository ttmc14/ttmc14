using Robust.Shared.GameStates;

namespace Content.Shared._MC.ASRS.Components;

/// <summary>
/// Defines the base supply point value of an item sold on an ASRS export pad.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(fieldDeltas: true)]
public sealed partial class MCASRSCostComponent : Component
{
    [DataField, AutoNetworkedField]
    public int Cost;
}
