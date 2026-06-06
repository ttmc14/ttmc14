using Content.Shared.Damage;
using Robust.Shared.GameStates;

namespace Content.Shared._MC.Xeno.Abilities.Runner.Pounce.FireCharge;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MCXenoPounceFireChargeComponent : Component
{
    [DataField, AutoNetworkedField]
    public DamageSpecifier DamagePerFireStack = new();
}
