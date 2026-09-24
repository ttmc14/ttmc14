using Robust.Shared.GameStates;

namespace Content.Shared._MC.Engineering.Miners.Components.Modules;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(fieldDeltas: true)]
public sealed partial class MCMinerModuleMinerProductionTimeComponent : Component
{
    [DataField, AutoNetworkedField]
    public TimeSpan? ReplaceTime;
}
