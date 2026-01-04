using Robust.Shared.GameStates;

namespace Content.Shared._MC.Xeno.Abilities.Shrike.UnrelentingForce;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MCXenoUnrelentingForceComponent : Component
{
    [DataField, AutoNetworkedField]
    public float ThrowSpeed = 15;
}
