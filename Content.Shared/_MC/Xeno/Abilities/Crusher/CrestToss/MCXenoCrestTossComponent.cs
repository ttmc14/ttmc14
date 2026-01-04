using Content.Shared.Damage;
using Robust.Shared.GameStates;

namespace Content.Shared._MC.Xeno.Abilities.Crusher.CrestToss;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(fieldDeltas: true)]
public sealed partial class MCXenoCrestTossComponent : Component
{
    [DataField, AutoNetworkedField]
    public DamageSpecifier Damage;

    [DataField, AutoNetworkedField]
    public float Distance = 6;

    [DataField, AutoNetworkedField]
    public float Speed = 10;
}
