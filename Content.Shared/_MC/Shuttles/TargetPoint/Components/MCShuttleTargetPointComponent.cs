using System.Numerics;
using Robust.Shared.GameStates;

namespace Content.Shared._MC.Shuttles.TargetPoint.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class MCShuttleTargetPointComponent : Component
{
    [DataField]
    public string Id = string.Empty;

    [DataField]
    public Vector2 Offset;
}
