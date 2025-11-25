using System.Numerics;
using Robust.Shared.GameStates;

namespace Content.Shared._MC.CommunicationsConsole;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MCERTCrashMarkerComponent : Component
{
    [DataField, AutoNetworkedField]
    public Vector2 Offset = new(0.5f, 0.5f);
}
