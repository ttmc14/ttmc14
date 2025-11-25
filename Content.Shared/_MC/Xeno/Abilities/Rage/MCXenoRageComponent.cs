using Robust.Shared.GameStates;

namespace Content.Shared._MC.Xeno.Abilities.Rage;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MCXenoRageComponent : Component
{
    [DataField, AutoNetworkedField]
    public float MinHealthThreshold = 0.75f;
}
