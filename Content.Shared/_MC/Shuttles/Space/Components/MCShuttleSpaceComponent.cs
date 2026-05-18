using Robust.Shared.GameStates;

namespace Content.Shared._MC.Shuttles.Space.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MCShuttleSpaceComponent : Component
{
    [DataField, AutoNetworkedField]
    public string Id;
}
