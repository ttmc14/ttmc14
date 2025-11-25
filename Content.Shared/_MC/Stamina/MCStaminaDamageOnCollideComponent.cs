using Robust.Shared.GameStates;

namespace Content.Shared._MC.Stamina;

[RegisterComponent, NetworkedComponent]
public sealed partial class MCStaminaDamageOnCollideComponent : Component
{
    [DataField]
    public float Damage;
}
