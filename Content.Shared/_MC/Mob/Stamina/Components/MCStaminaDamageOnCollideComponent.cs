using Robust.Shared.GameStates;

namespace Content.Shared._MC.Mob.Stamina.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(fieldDeltas: true)]
public sealed partial class MCStaminaDamageOnCollideComponent : Component
{
    [DataField, AutoNetworkedField]
    public float Damage;
}
