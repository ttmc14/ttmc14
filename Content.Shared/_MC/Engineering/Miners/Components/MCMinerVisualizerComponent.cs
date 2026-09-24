using Robust.Shared.GameStates;
using Robust.Shared.Utility;

namespace Content.Shared._MC.Engineering.Miners.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(fieldDeltas: true)]
public sealed partial class MCMinerVisualizerComponent : Component
{
    [DataField, AutoNetworkedField]
    public ResPath Sprite = ResPath.Empty;

    [DataField, AutoNetworkedField]
    public Dictionary<MCMinerState, string> StatesState = new()
    {
        { MCMinerState.Running, "-active" },
        { MCMinerState.SmallDamage, "-braced" },
        { MCMinerState.MediumDamage, string.Empty },
        { MCMinerState.Destroyed, "-error" },
    };
}
