using Content.Shared.Damage;
using Robust.Shared.GameStates;
using Robust.Shared.Map;

namespace Content.Shared._MC.Xeno.Abilities.Wraith.Rewind;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MCXenoRewindTargetComponent : Component
{
    [DataField, AutoNetworkedField]
    public bool Canceled;

    [DataField, AutoNetworkedField]
    public EntityCoordinates Position;

    [DataField, AutoNetworkedField]
    public DamageSpecifier? Damage;

    [DataField, AutoNetworkedField]
    public float FireStacks;
}
