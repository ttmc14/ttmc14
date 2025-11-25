using Content.Shared._MC.Bomb.Components;
using Content.Shared._MC.Bomb.UI;
using JetBrains.Annotations;
using Robust.Client.UserInterface;

namespace Content.Client._MC.Bomb.UI;

[UsedImplicitly]
public sealed class BombPasswordBui : BoundUserInterface
{
    [ViewVariables]
    private BombPasswordWindow? _window;

    public BombPasswordBui(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
    }

    protected override void Open()
    {
        base.Open();

        _window = this.CreateWindow<BombPasswordWindow>();

        // Number buttons
        _window.Button0.OnPressed += _ => SendMessage(new BombPasswordDigitBuiMessage(0));
        _window.Button1.OnPressed += _ => SendMessage(new BombPasswordDigitBuiMessage(1));
        _window.Button2.OnPressed += _ => SendMessage(new BombPasswordDigitBuiMessage(2));
        _window.Button3.OnPressed += _ => SendMessage(new BombPasswordDigitBuiMessage(3));
        _window.Button4.OnPressed += _ => SendMessage(new BombPasswordDigitBuiMessage(4));
        _window.Button5.OnPressed += _ => SendMessage(new BombPasswordDigitBuiMessage(5));
        _window.Button6.OnPressed += _ => SendMessage(new BombPasswordDigitBuiMessage(6));
        _window.Button7.OnPressed += _ => SendMessage(new BombPasswordDigitBuiMessage(7));
        _window.Button8.OnPressed += _ => SendMessage(new BombPasswordDigitBuiMessage(8));
        _window.Button9.OnPressed += _ => SendMessage(new BombPasswordDigitBuiMessage(9));

        // Action buttons
        _window.ClearButton.OnPressed += _ => SendMessage(new BombPasswordClearBuiMessage());
        _window.SetButton.OnPressed += _ => SendMessage(new BombPasswordSetBuiMessage());
        _window.ResetButton.OnPressed += _ => SendMessage(new BombPasswordResetBuiMessage());
        _window.RandomButton.OnPressed += _ => SendMessage(new BombPasswordRandomBuiMessage());
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);

        if (_window is null)
            return;

        switch (state)
        {
            case BombPasswordBuiState mainState:
                SetState(mainState);
                break;
        }
    }

    private void SetState(BombPasswordBuiState state)
    {
        if (_window is null)
            return;

        _window.SetDisplay(state.CurrentInput);

        // Note: `BombPasswordWindow` does not implement `ApplyState` in this rollback.
        // If UI highlighting/progress is needed, implement `ApplyState` in the window
        // or update this method to call the appropriate window API.
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
        {
            _window?.Dispose();
        }
    }
}

