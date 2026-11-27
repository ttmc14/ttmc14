using Robust.Shared.Serialization;

namespace Content.Shared._MC.Nuke.Bomb.UI;

[Serializable, NetSerializable]
public sealed class MCNukeDiskBuiMessage(string slot) : BoundUserInterfaceMessage
{
    public readonly string Slot = slot;
}
