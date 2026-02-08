using Robust.Shared.Serialization;

namespace Content.Shared._MC.Bomb.UI;

[Serializable, NetSerializable]
public sealed class MCBombPasswordBuiState : BoundUserInterfaceState
{
    public readonly string CurrentInput;
    public readonly bool PasswordSet;
    public readonly bool Unlocked;

    public MCBombPasswordBuiState(string currentInput, bool passwordSet, bool unlocked)
    {
        CurrentInput = currentInput;
        PasswordSet = passwordSet;
        Unlocked = unlocked;
    }
}
