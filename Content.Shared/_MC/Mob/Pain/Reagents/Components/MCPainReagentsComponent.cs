using Robust.Shared.GameStates;

namespace Content.Shared._MC.Mob.Pain.Reagents.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(fieldDeltas: true)]
public sealed partial class MCPainReagentsComponent : Component
{
    [ViewVariables, AutoNetworkedField]
    public float Painloss;
}
