using Robust.Shared.GameStates;

namespace Content.Shared._MC.Xeno.Abilities.Ravager.Rage.Components.Activation;

[RegisterComponent, NetworkedComponent]
public sealed partial class MCXenoRageActivationHealthComponent : Component
{
    [DataField]
    public float MinHealthThreshold = 0.5f;

    [ViewVariables]
    public bool SpecialUsed;
}
