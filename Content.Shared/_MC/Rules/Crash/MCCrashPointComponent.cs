using System.Numerics;
using Robust.Shared.GameStates;

namespace Content.Shared._MC.Rules.Crash;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MCCrashPointComponent : Component
{
    [DataField, AutoNetworkedField]
    public Vector2 Offset = new(0.5f, 0.5f);
}
