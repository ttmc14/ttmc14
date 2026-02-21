using Robust.Shared.Serialization;

namespace Content.Shared._MC.Marine.Customization.Gui;

[Serializable, NetSerializable]
public sealed class MCCustomizationSelectBuiMessage : BoundUserInterfaceMessage
{
    public readonly string Key;
    public readonly NetEntity TargetUid;

    public MCCustomizationSelectBuiMessage(string key, NetEntity targetUid)
    {
        Key = key;
        TargetUid = targetUid;
    }
}
