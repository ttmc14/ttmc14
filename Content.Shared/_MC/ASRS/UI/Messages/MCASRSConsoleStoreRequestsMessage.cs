using Robust.Shared.Serialization;

namespace Content.Shared._MC.ASRS.UI.Messages;

[Serializable, NetSerializable]
public sealed class MCASRSConsoleStoreRequestsMessage(string reason, Dictionary<int, int> quantities) : BoundUserInterfaceMessage
{
    public readonly string Reason = reason;
    public readonly Dictionary<int, int> Quantities = quantities;
}
