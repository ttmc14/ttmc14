using Content.Shared._MC.Knockback;
using Robust.Shared.GameStates;

namespace Content.Shared._MC.Xeno.Abilities.Runner.Pounce.Knockback;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MCXenoPounceKnockbackComponent : Component
{
    [DataField, AutoNetworkedField]
    public MCKnockbackEntry Entry = new();
}
