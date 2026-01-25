using Content.Shared.Damage;
using Robust.Shared.GameStates;

namespace Content.Shared._MC.ZLevels;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(fieldDeltas: true)]
public sealed partial class MCZLevelFallDamageComponent : Component
{
    [DataField(required: true), AutoNetworkedField]
    public DamageSpecifier Damage = new();
}
