using Robust.Shared.GameStates;

namespace Content.Shared._MC.Shuttle.Console;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MCShuttleConsoleComponent : Component
{
    [AutoNetworkedField]
    public EntityUid? Shuttle;
}
