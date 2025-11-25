using Robust.Shared.Serialization;

namespace Content.Shared._MC.Bomb.UI;

[Serializable, NetSerializable]
public sealed class BombPasswordDigitBuiMessage : BoundUserInterfaceMessage
{
    public readonly int Digit;

    public BombPasswordDigitBuiMessage(int digit)
    {
        Digit = digit;
    }
}

