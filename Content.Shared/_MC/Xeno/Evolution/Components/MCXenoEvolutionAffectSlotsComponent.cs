using Robust.Shared.GameStates;

namespace Content.Shared._MC.Xeno.Evolution.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(fieldDeltas: true)]
public sealed partial class MCXenoEvolutionAffectSlotsComponent : Component
{
    [DataField, AutoNetworkedField]
    public Dictionary<int, int> Slots = new();
}
