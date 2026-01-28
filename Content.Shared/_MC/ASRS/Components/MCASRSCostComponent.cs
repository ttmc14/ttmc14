using Robust.Shared.GameStates;

namespace Content.Shared._MC.ASRS.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(fieldDeltas: true)]
public sealed partial class MCASRSCostComponent : Component
{
    [DataField, AutoNetworkedField]
    public int Cost;
}
