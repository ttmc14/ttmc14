using Content.Server.Defusable.Components;
using Content.Server.Defusable.Systems;
using Content.Server.Popups;
using Content.Shared._MC.Bomb.Components;
using Content.Shared._MC.Bomb.UI;
using Content.Shared.Popups;
using Content.Shared.Sticky;
using Robust.Server.GameObjects;
using Robust.Shared.Random;
using System.Linq;

namespace Content.Server._MC.Bomb.Systems;

public sealed class BombPasswordSystem : EntitySystem
{
    [Dependency] private readonly SharedUserInterfaceSystem _userInterface = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly DefusableSystem _defusable = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BombPasswordComponent, BombPasswordDigitBuiMessage>(OnDigitMessage);
        SubscribeLocalEvent<BombPasswordComponent, BombPasswordClearBuiMessage>(OnClearMessage);
        SubscribeLocalEvent<BombPasswordComponent, BombPasswordSetBuiMessage>(OnSetMessage);
        SubscribeLocalEvent<BombPasswordComponent, BombPasswordResetBuiMessage>(OnResetMessage);
        SubscribeLocalEvent<BombPasswordComponent, BombPasswordRandomBuiMessage>(OnRandomMessage);
        SubscribeLocalEvent<BombPasswordComponent, ComponentInit>(OnComponentInit);
        SubscribeLocalEvent<BombPasswordComponent, AttemptEntityStickEvent>(OnAttemptStick);
        SubscribeLocalEvent<BombPasswordComponent, BombDefusedEvent>(OnBombDefused);
    }

    private void OnComponentInit(Entity<BombPasswordComponent> ent, ref ComponentInit args)
    {
        UpdateUi(ent);
    }

    private void OnDigitMessage(Entity<BombPasswordComponent> ent, ref BombPasswordDigitBuiMessage args)
    {
        var comp = ent.Comp;

        // Check if bomb is activated - if not, allow input for setting new password
        var isActivated = TryComp<DefusableComponent>(ent.Owner, out var defusableComp) && defusableComp.Activated;

        // Don't allow input if password is set, unlocked, and bomb is activated
        if (comp.PasswordSet && comp.Unlocked && isActivated)
            return;

        // Don't allow input if already at max length
        if (comp.CurrentInput.Length >= comp.MaxLength)
            return;

        // Add digit
        comp.CurrentInput += args.Digit.ToString();
        UpdateUi(ent);
    }

    private void OnClearMessage(Entity<BombPasswordComponent> ent, ref BombPasswordClearBuiMessage args)
    {
        var comp = ent.Comp;

        // Check if bomb is activated
        var isActivated = TryComp<DefusableComponent>(ent.Owner, out var defusableComp) && defusableComp.Activated;

        // Don't allow clearing if password is set, unlocked, and bomb is activated
        if (comp.PasswordSet && comp.Unlocked && isActivated)
            return;

        // Remove last digit
        if (comp.CurrentInput.Length > 0)
        {
            comp.CurrentInput = comp.CurrentInput[..^1];
            UpdateUi(ent);
        }
    }

    private void OnSetMessage(Entity<BombPasswordComponent> ent, ref BombPasswordSetBuiMessage args)
    {
        var comp = ent.Comp;

        // If password is already set and currently unlocked, allow replacing it
        // only when the bomb is not activated. This lets the owner change the
        // password without having to press Reset first.
        if (comp.PasswordSet && comp.Unlocked)
        {
            var isActivatedReplace = TryComp<DefusableComponent>(ent.Owner, out var defusableReplace) && defusableReplace.Activated;

            // Do not allow replacing while activated
            if (isActivatedReplace)
                return;

            // Require full length for new password
            if (comp.CurrentInput.Length < comp.MaxLength)
            {
                _popup.PopupEntity(Loc.GetString("bomb-password-too-short"), ent, PopupType.MediumCaution);
                return;
            }

            comp.Password = comp.CurrentInput;
            comp.CurrentInput = string.Empty;
            comp.PasswordSet = true;
            // After setting a new password, consider it locked again.
            comp.Unlocked = false;
            _popup.PopupEntity(Loc.GetString("bomb-password-set"), ent, PopupType.Medium);
            UpdateUi(ent);
            return;
        }

        // If password is already set and not unlocked, try to unlock with current input
        if (comp.PasswordSet && !comp.Unlocked)
        {
            if (comp.CurrentInput == comp.Password)
            {
                comp.Unlocked = true;
                comp.CurrentInput = string.Empty;
                _popup.PopupEntity(Loc.GetString("bomb-password-unlocked"), ent, PopupType.Medium);
                UpdateUi(ent);

                // If bomb is activated, defuse it automatically
                if (TryComp<DefusableComponent>(ent.Owner, out var defusableComp) && defusableComp.Activated)
                {
                    _defusable.TryDefuseBomb(ent.Owner, defusableComp);
                    // Password state will be reset in OnBombDefused
                }
            }
            else
            {
                comp.CurrentInput = string.Empty;
                _popup.PopupEntity(Loc.GetString("bomb-password-incorrect"), ent, PopupType.MediumCaution);
                UpdateUi(ent);
            }
            return;
        }

        // If password is not set, set it
        if (!comp.PasswordSet)
        {
            if (comp.CurrentInput.Length < comp.MaxLength)
            {
                _popup.PopupEntity(Loc.GetString("bomb-password-too-short"), ent, PopupType.MediumCaution);
                return;
            }

            comp.Password = comp.CurrentInput;
            comp.PasswordSet = true;
            comp.CurrentInput = string.Empty;
            _popup.PopupEntity(Loc.GetString("bomb-password-set"), ent, PopupType.Medium);
            UpdateUi(ent);
        }
    }

    private void OnResetMessage(Entity<BombPasswordComponent> ent, ref BombPasswordResetBuiMessage args)
    {
        var comp = ent.Comp;

        // Can reset password if bomb is not activated (either never activated or defused)
        var isActivated = TryComp<DefusableComponent>(ent.Owner, out var defusableComp) && defusableComp.Activated;

        if (isActivated)
        {
            _popup.PopupEntity(Loc.GetString("bomb-password-cannot-reset-active"), ent, PopupType.MediumCaution);
            return;
        }

        comp.Password = null;
        comp.PasswordSet = false;
        comp.Unlocked = false;
        comp.CurrentInput = string.Empty;
        _popup.PopupEntity(Loc.GetString("bomb-password-reset"), ent, PopupType.Medium);
        UpdateUi(ent);
    }

    private void OnRandomMessage(Entity<BombPasswordComponent> ent, ref BombPasswordRandomBuiMessage args)
    {
        var comp = ent.Comp;

        // Don't allow random if password is already set
        if (comp.PasswordSet)
            return;

        // Generate random numeric password of length `MaxLength` and show it in input field.
        // User needs to press SET to confirm it.
        var upper = (int)Math.Pow(10, comp.MaxLength);
        var randomPassword = _random.Next(0, upper).ToString($"D{comp.MaxLength}");
        comp.CurrentInput = randomPassword;
        _popup.PopupEntity(Loc.GetString("bomb-password-random-generated", ("password", randomPassword)), ent, PopupType.Medium);
        UpdateUi(ent);
    }

    private void UpdateUi(Entity<BombPasswordComponent> ent)
    {
        var comp = ent.Comp;
        string displayInput;
        var max = comp.MaxLength;

        if (comp.PasswordSet && !comp.Unlocked)
        {
            // If password is set and not unlocked, show entered digits and masked remainder with spaced '*'
            var entered = comp.CurrentInput.Length;
            var enteredSpaced = entered > 0 ? string.Join(' ', comp.CurrentInput.ToCharArray()) : string.Empty;
            var remaining = Math.Max(0, max - entered);

            if (entered > 0)
            {
                if (remaining > 0)
                {
                    var stars = string.Join(' ', Enumerable.Repeat("*", remaining));
                    displayInput = enteredSpaced + " " + stars;
                }
                else
                {
                    displayInput = enteredSpaced;
                }
            }
            else
            {
                displayInput = string.Join(' ', Enumerable.Repeat("*", max));
            }
        }
        else
        {
            // If password is not set or it's unlocked, show current input followed by spaced '_' placeholders
            var entered = comp.CurrentInput.Length;
            var enteredSpaced = entered > 0 ? string.Join(' ', comp.CurrentInput.ToCharArray()) : string.Empty;
            var remaining = Math.Max(0, max - entered);

            if (entered > 0)
            {
                if (remaining > 0)
                {
                    var underscores = string.Join(' ', Enumerable.Repeat("_", remaining));
                    displayInput = enteredSpaced + " " + underscores;
                }
                else
                {
                    displayInput = enteredSpaced;
                }
            }
            else
            {
                displayInput = string.Join(' ', Enumerable.Repeat("_", max));
            }
        }

        _userInterface.SetUiState(ent.Owner, BombPasswordUi.Key,
            new BombPasswordBuiState(displayInput, comp.PasswordSet, comp.Unlocked));
    }

    /// <summary>
    /// Check if the bomb can be activated (password must be set).
    /// </summary>
    public bool CanActivate(Entity<BombPasswordComponent> ent)
    {
        // Only allow activation if a password is set and it is currently locked.
        // If the password has been unlocked (comp.Unlocked == true), the bomb
        // must not be activatable until a new password is set again.
        return ent.Comp.PasswordSet && !ent.Comp.Unlocked;
    }

    /// <summary>
    /// Check if the bomb can be defused (password must be unlocked).
    /// </summary>
    public bool CanDefuse(Entity<BombPasswordComponent> ent)
    {
        return ent.Comp.Unlocked;
    }

    private void OnAttemptStick(Entity<BombPasswordComponent> ent, ref AttemptEntityStickEvent args)
    {
        var comp = ent.Comp;

        // Don't allow sticking if password is not set
        if (!comp.PasswordSet)
        {
            _popup.PopupEntity(Loc.GetString("bomb-password-cannot-stick"), ent, args.User, PopupType.MediumCaution);
            args.Cancelled = true;
        }
    }

    private void OnBombDefused(Entity<BombPasswordComponent> ent, ref BombDefusedEvent args)
    {
        var comp = ent.Comp;

        // After bomb is defused, reset password state to allow setting new password
        comp.Password = null;
        comp.PasswordSet = false;
        comp.Unlocked = false;
        comp.CurrentInput = string.Empty;
        UpdateUi(ent);
    }
}

