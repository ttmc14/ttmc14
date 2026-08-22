using Robust.Shared.GameStates;

namespace Content.Shared._MC.Xeno.Abilities.Ravager.Rage.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class MCXenoRageAuraComponent : Component
{
    [DataField]
    public Color AuraColor = Color.Red;

    [DataField]
    public float AuraStrength = 3;
}
