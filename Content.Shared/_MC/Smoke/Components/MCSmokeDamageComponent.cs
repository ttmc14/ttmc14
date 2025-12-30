using Content.Shared.Damage;
using Robust.Shared.GameStates;

namespace Content.Shared._MC.Smoke.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(fieldDeltas: true)]
public sealed partial class MCSmokeDamageComponent : Component
{
    [DataField, AutoNetworkedField]
    public DamageSpecifier Damage = new();
}
