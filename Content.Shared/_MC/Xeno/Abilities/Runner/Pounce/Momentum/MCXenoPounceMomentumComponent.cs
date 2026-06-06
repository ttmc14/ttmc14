using Robust.Shared.GameStates;

namespace Content.Shared._MC.Xeno.Abilities.Runner.Pounce.Momentum;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MCXenoPounceMomentumComponent : Component
{
    [DataField, AutoNetworkedField]
    public int Gain = 2;
}
