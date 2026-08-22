using Robust.Shared.GameStates;

namespace Content.Shared._MC.Xeno.Abilities.Ravager.Rage.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class MCXenoRageActiveComponent : Component
{
    [ViewVariables]
    public float Power;
}
