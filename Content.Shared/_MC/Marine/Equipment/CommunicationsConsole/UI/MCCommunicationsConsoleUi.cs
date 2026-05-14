using Robust.Shared.Serialization;

namespace Content.Shared._MC.CommunicationsConsole.UI;

[Serializable, NetSerializable]
public sealed class MCCommunicationsConsoleBuiState : BoundUserInterfaceState;

[Serializable, NetSerializable]
public enum MCCommunicationsConsoleUi
{
    Key,
}
