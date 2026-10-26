using System.Numerics;
using Robust.Shared.GameStates;

namespace Content.Shared._MC.Shuttle;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MCShuttlePointComponent : Component
{
    [DataField, AutoNetworkedField]
    public Vector2 Offset = new(0.5f, 0.5f);
}
