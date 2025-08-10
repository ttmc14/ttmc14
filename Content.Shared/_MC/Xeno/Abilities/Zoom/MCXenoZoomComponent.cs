using System.Numerics;
using Robust.Shared.GameStates;

namespace Content.Shared._MC.Xeno.Abilities.Zoom;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MCXenoZoomComponent : Component
{
    [DataField, AutoNetworkedField]
    public Vector2 Zoom = new(1.25f, 1.25f);

    [DataField, AutoNetworkedField]
    public int OffsetLength;

    [DataField, AutoNetworkedField]
    public float Speed = 1;

    [DataField, AutoNetworkedField]
    public TimeSpan DoAfter = TimeSpan.Zero;

    [DataField, AutoNetworkedField]
    public bool CanMove;
}
