using Robust.Shared.Serialization;

namespace Content.Shared._MC.Bomb.UI;

[Serializable, NetSerializable]
public sealed class BombPasswordBuiState : BoundUserInterfaceState
{
    public readonly string CurrentInput;
    public readonly bool PasswordSet;
    public readonly bool Unlocked;

    public BombPasswordBuiState(string currentInput, bool passwordSet, bool unlocked)
    {
        CurrentInput = currentInput;
        PasswordSet = passwordSet;
        Unlocked = unlocked;
    }
}

