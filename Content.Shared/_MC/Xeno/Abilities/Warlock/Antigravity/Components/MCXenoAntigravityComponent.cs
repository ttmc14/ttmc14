using Robust.Shared.GameStates;

namespace Content.Shared._MC.Xeno.Abilities.Warlock.Antigravity.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(fieldDeltas: true)]
public sealed partial class MCXenoAntigravityComponent : Component
{
    [DataField, AutoNetworkedField]
    public int ZLevels = 7;

    [DataField, AutoNetworkedField]
    public int TargetMapHeight;

    [DataField, AutoNetworkedField]
    public bool Active = true;

    [DataField, AutoNetworkedField]
    public float Speed = 1.5f;
}
