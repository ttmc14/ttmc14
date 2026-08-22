using Robust.Shared.GameStates;

namespace Content.Shared._MC.UserInterface.Ensuring;

[RegisterComponent, NetworkedComponent]
public sealed partial class MCUserInterfaceEnsuringComponent : Component
{
    [DataField]
    public Dictionary<Enum, InterfaceData> Interfaces = new();
}
