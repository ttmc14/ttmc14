using Robust.Shared.Serialization;

namespace Content.Shared._MC.Bomb.UI;

/// <summary>
/// Sent by client when using a Pulsing tool to attempt entering the bomb password by sequence.
/// Contains the digit that was clicked.
/// </summary>
[Serializable, NetSerializable]
public sealed class MCBombPasswordToolSequenceBuiMessage : BoundUserInterfaceMessage
{
    public readonly int Digit;

    public MCBombPasswordToolSequenceBuiMessage(int digit)
    {
        Digit = digit;
    }
}
