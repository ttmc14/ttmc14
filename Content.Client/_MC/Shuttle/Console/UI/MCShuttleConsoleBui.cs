using Content.Shared._MC.Shuttle.Console.UI;
using JetBrains.Annotations;
using Robust.Client.UserInterface;

namespace Content.Client._MC.Shuttle.Console.UI;

[UsedImplicitly]
public sealed class MCShuttleConsoleBui(EntityUid owner, Enum uiKey) : BoundUserInterface(owner, uiKey)
{
    [ViewVariables]
    private MCShuttleConsoleWindow? _window;

    protected override void Open()
    {
        base.Open();

        _window = this.CreateWindow<MCShuttleConsoleWindow>();
        _window.Launch.OnPressed += _ => SendMessage(new MCShuttleConsoleEvacuateBuiMessage(false));
        _window.EarlyLaunch.OnPressed += _ => SendMessage(new MCShuttleConsoleEvacuateBuiMessage(true));
        if (State is MCShuttleConsoleState state)
        {
            UpdateState(state);
        }
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        if (_window == null || state is not MCShuttleConsoleState evacuation)
            return;

        _window.Status.Text = Loc.GetString($"mc-evac-status-{evacuation.Status}", ("seconds", evacuation.Seconds));
        _window.EarlyLaunch.Visible = evacuation.Status == "early";
        _window.Launch.Visible = !_window.EarlyLaunch.Visible;
        _window.Launch.Disabled = evacuation.Status != "ready";
    }
}
