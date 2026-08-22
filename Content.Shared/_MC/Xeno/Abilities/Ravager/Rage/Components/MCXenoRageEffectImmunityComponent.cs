using Robust.Shared.GameStates;

namespace Content.Shared._MC.Xeno.Abilities.Ravager.Rage.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class MCXenoRageEffectImmunityComponent : Component
{
    [DataField]
    public float StaggerImmuneThreshold = 0.5f;

    [DataField]
    public float StunImmuneThreshold = 0.5f;
}
