using Robust.Shared.GameStates;

namespace Content.Shared._MC.Engineering.Miners.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(fieldDeltas: true)]
public sealed partial class MCMinerModuleComponent : Component
{
    [DataField, AutoNetworkedField]
    public string Appearance;
}
