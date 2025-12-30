using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._MC.Xeno.Abilities.Defiler.EmitNeurogas;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MCXenoEmitNeurogasActiveComponent : Component
{
    [DataField, AutoNetworkedField]
    public EntProtoId SmokeId;

    [DataField, AutoNetworkedField]
    public TimeSpan ActivationDelay;

    [DataField, AutoNetworkedField]
    public TimeSpan ActivationTimeNext;

    [DataField, AutoNetworkedField]
    public int Activations;

    [DataField, AutoNetworkedField]
    public int Range;
}
