using Robust.Shared.Serialization;

namespace Content.Shared._MC.ASRS.UI.Messages.Deny;

[Serializable, NetSerializable]
public sealed class MCASRSConsoleDenyMessage : BoundUserInterfaceMessage
{
    public readonly Guid RequestId;

    public MCASRSConsoleDenyMessage(Guid requestId)
    {
        RequestId = requestId;
    }
}
