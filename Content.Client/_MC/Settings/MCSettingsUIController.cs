using JetBrains.Annotations;
using Robust.Client.UserInterface.Controllers;
using Robust.Shared.Console;

namespace Content.Client._MC.Settings;

[UsedImplicitly]
public sealed class MCSettingsUIController : UIController
{
    [Dependency] private readonly IConsoleHost _consoleHost = null!;

    private MCSettingsMenu _window = null!;

    public override void Initialize()
    {
        _consoleHost.RegisterCommand("mc_options", Loc.GetString("mc-cmd-options-desc"), Loc.GetString("mc-cmd-options-help"), Command);
    }

    private void Command(IConsoleShell shell, string argStr, string[] args)
    {
        if (args.Length == 0)
        {
            ToggleWindow();
            return;
        }
        OpenWindow();

        if (!int.TryParse(args[0], out var tab))
        {
            shell.WriteError(Loc.GetString("cmd-parse-failure-integer", ("arg", args[0])));
            return;
        }

        _window.Tabs.CurrentTab = tab;
    }

    private void EnsureWindow()
    {
        if (_window is { Disposed: false })
            return;

        _window = UIManager.CreateWindow<MCSettingsMenu>();
    }

    public void OpenWindow()
    {
        EnsureWindow();

        _window.UpdateTabs();
        _window.OpenCentered();
        _window.MoveToFront();
    }

    public void ToggleWindow()
    {
        EnsureWindow();

        if (_window.IsOpen)
        {
            _window.Close();
            return;
        }

        OpenWindow();
    }
}
