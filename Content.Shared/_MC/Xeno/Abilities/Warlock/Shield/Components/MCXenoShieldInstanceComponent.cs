using Robust.Shared.GameStates;

namespace Content.Shared._MC.Xeno.Abilities.Warlock.Shield.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(fieldDeltas: true)]
public sealed partial class MCXenoShieldInstanceComponent : Component
{
    [ViewVariables]
    public List<EntityUid> Projectiles = new();

    [ViewVariables, AutoNetworkedField]
    public EntityUid OwnerUid;

    [ViewVariables, AutoNetworkedField]
    public bool Terminating;

    [DataField, AutoNetworkedField]
    public float Integrity = 350f;

    [DataField, AutoNetworkedField]
    public float IntegrityMax = 350f;
}
