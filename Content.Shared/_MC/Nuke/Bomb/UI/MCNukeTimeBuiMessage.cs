using Robust.Shared.Serialization;

namespace Content.Shared._MC.Nuke.Bomb.UI;

[Serializable, NetSerializable]
public sealed class MCNukeTimeBuiMessage(int seconds) : BoundUserInterfaceMessage
{
    public readonly int Seconds = seconds;
}
