using Robust.Shared.GameStates;

namespace Content.Shared._MC.Bomb.Components;

/// <summary>
/// Component for storing and managing password on bombs.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class MCBombPasswordComponent : Component
{
    /// <summary>
    /// The actual password (4 digits, stored as string for easier comparison).
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    public string? Password;

    /// <summary>
    /// Current input being entered by the user.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    public string CurrentInput = string.Empty;

    /// <summary>
    /// Whether the password has been set and confirmed.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    public bool PasswordSet;

    /// <summary>
    /// Whether the password is currently unlocked (correct password entered).
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    public bool Unlocked;

    /// <summary>
    /// Maximum length of the password.
    /// Expanded to 7 digits per request.
    /// </summary>
    [DataField("maxLength")]
    public int MaxLength = 7;
}
