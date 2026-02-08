using Robust.Shared.Serialization;

namespace Content.Shared._MC.Bomb.UI;

[Serializable, NetSerializable]
public sealed class MCBombPasswordDigitBuiMessage : BoundUserInterfaceMessage
{
    public readonly int Digit;

    public MCBombPasswordDigitBuiMessage(int digit)
    {
        Digit = digit;
    }
}
