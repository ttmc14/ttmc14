using Content.Shared._MC.Nuke.Bomb.Components;
using Content.Shared._MC.Nuke.Bomb.UI;
using JetBrains.Annotations;
using Robust.Client.UserInterface;

namespace Content.Client._MC.Nuke.Bomb.UI;

[UsedImplicitly]
public sealed class MCNukeBui : BoundUserInterface
{
    [Dependency] private readonly IEntityManager _entities = default!;

    [ViewVariables]
    private MCNukeWindow? _window;

    public MCNukeBui(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
    }

    protected override void Open()
    {
        base.Open();

        _window = this.CreateWindow<MCNukeWindow>();

        _window.TimerButton.OnPressed += _ => SendMessage(new MCNukeActiveBuiMessage());
        _window.SafetyButton.OnPressed += _ => SendMessage(new MCNukeSafetyBuiMessage());
        _window.AnchorButton.OnPressed += _ => SendMessage(new MCNukeAnchorBuiMessage());

        if (!_entities.TryGetComponent<MCNukeComponent>(Owner, out var component))
            return;

        SetTime(component.Time);
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);

        if (_window is null)
            return;

        switch (state)
        {
            case MCNukeBuiState mainState:
                SetState(mainState);
                break;
        }
    }

    private void SetState(MCNukeBuiState state)
    {
        if (_window is null)
           return;

        // Ready
        _window.SettingsBoxContainer.Visible = state.Ready;

        // Safety
        _window.SafetyButton.Text = state.Safety ? "Disable" : "Enable";

        _window.TimerButton.Text = state.Activated ? "DEACTIVATE" : "ACTIVATE";
        _window.TimerButton.Disabled = state is { Safety: true, Anchored: true };

        // Anchored
        _window.AnchorButton.Text = state.Anchored ? "Unanchor" : "Anchor";
    }

    private void SetTime(TimeSpan time)
    {
        if (_window is null)
            return;

        _window.TimeLabel.Text = $"{time}s";
    }
}
