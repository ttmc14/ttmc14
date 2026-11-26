using Robust.Shared.Serialization;

namespace Content.Shared._MC.ASRS.UI.Messages.Delivery;

[Serializable, NetSerializable]
public sealed class MCASRSConsoleDeliveryMessage : BoundUserInterfaceMessage
{
    public readonly Guid RequestId;
    public readonly NetEntity BeaconUid;

    public MCASRSConsoleDeliveryMessage(Guid requestId, NetEntity beaconUid)
    {
        RequestId = requestId;
        BeaconUid = beaconUid;
    }
}
