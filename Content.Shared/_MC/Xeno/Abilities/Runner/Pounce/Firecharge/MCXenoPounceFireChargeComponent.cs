using Content.Shared.Damage;
using Robust.Shared.GameStates;

namespace Content.Shared._MC.Xeno.Abilities.Pounce.Firecharge;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MCXenoPounceFireChargeComponent : Component
{
    [DataField, AutoNetworkedField]
    public DamageSpecifier DamagePerFireStack = new();
}
