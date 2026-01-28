using Content.Shared.Chemistry.Reagent;
using Robust.Shared.GameStates;

namespace Content.Shared._MC.Xeno.Abilities.Sentinel.NeurotoxinSting;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(fieldDeltas: true)]
public sealed partial class MCXenoNeurotoxinStingComponent : Component
{
    [DataField, AutoNetworkedField]
    public TimeSpan Delay = TimeSpan.FromSeconds(0.25);

    [DataField, AutoNetworkedField]
    public int Count = 4;

    [DataField, AutoNetworkedField]
    public float Range = 1.5f;

    [DataField, AutoNetworkedField]
    public string Solution = "chemicals";

    [DataField, AutoNetworkedField]
    public List<ReagentQuantity> Reagents = new();
}
