using System.Numerics;
using Robust.Shared.GameStates;

namespace Content.Shared._MC.Xeno.Abilities.Zoom;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MCXenoZoomActiveComponent : Component
{
    [ViewVariables, AutoNetworkedField]
    public Vector2 Zoom = new(1.25f, 1.25f);

    [ViewVariables, AutoNetworkedField]
    public Vector2 Offset;

    [ViewVariables, AutoNetworkedField]
    public float Speed;

    [ViewVariables, AutoNetworkedField]
    public bool CanMove;
}
