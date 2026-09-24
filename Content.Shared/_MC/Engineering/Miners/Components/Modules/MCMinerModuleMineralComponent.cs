using Robust.Shared.GameStates;

namespace Content.Shared._MC.Engineering.Miners.Components.Modules;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(fieldDeltas: true)]
public sealed partial class MCMinerModuleMineralComponent : Component
{
    [DataField, AutoNetworkedField]
    public int AdjustedValue;
}
