using Robust.Shared.Serialization;

namespace Content.Shared._MC.ASRS.Ui;

[Serializable, NetSerializable]
public sealed class MCASRSConsoleApproveMessage : BoundUserInterfaceMessage
{
    public readonly MCASRSRequest Request;

    public MCASRSConsoleApproveMessage(MCASRSRequest request)
    {
        Request = request;
    }
}
