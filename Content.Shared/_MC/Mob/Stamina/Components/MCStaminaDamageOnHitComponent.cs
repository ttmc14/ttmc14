using Robust.Shared.GameStates;

namespace Content.Shared._MC.Mob.Stamina.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(fieldDeltas: true)]
public sealed partial class MCStaminaDamageOnHitComponent : Component
{
    [DataField, AutoNetworkedField]
    public float Damage;

    [DataField, AutoNetworkedField]
    public bool RequiresWield;
}
