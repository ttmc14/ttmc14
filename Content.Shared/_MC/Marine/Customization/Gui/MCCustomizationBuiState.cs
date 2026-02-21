using Robust.Shared.Serialization;

namespace Content.Shared._MC.Marine.Customization.Gui;

[Serializable, NetSerializable]
public sealed class MCCustomizationBuiState : BoundUserInterfaceState
{
    public readonly Dictionary<string, MCCustomizationVariationData> Data;
    public readonly NetEntity TargetUid;

    public MCCustomizationBuiState(Dictionary<string, MCCustomizationVariationData> data, NetEntity targetUid)
    {
        Data = data;
        TargetUid = targetUid;
    }
}
