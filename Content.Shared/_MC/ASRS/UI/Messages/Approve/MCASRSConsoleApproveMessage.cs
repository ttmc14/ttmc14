using Robust.Shared.Serialization;

namespace Content.Shared._MC.ASRS.UI.Messages.Approve;

[Serializable, NetSerializable]
public sealed class MCASRSConsoleApproveMessage : BoundUserInterfaceMessage
{
    public readonly Guid RequestId;

    public MCASRSConsoleApproveMessage(Guid requestId)
    {
        RequestId = requestId;
    }
}
