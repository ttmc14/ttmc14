using Robust.Shared.Serialization;

namespace Content.Shared._MC.Shuttle.Console.UI;

[Serializable, NetSerializable]
public sealed class MCShuttleConsoleEvacuateBuiMessage(bool confirmed) : BoundUserInterfaceMessage
{
    public readonly bool Confirmed = confirmed;
}
