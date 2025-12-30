using Robust.Shared.GameStates;

namespace Content.Shared._MC.Xeno.Abilities.Ravager.Rage;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MCXenoRageComponent : Component
{
    [DataField, AutoNetworkedField]
    public float MinHealthThreshold = 0.75f;
}
