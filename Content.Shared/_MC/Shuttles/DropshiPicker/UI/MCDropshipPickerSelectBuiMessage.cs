using Robust.Shared.Serialization;

namespace Content.Shared._MC.Shuttles.DropshiPicker.UI;

[Serializable, NetSerializable]
public sealed class MCDropshipPickerSelectBuiMessage(string path) : BoundUserInterfaceMessage
{
    public readonly string Path = path;
}
