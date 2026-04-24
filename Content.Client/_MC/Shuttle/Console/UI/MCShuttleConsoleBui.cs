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
        _window.EvacuationButton.OnPressed += _ => SendMessage(new MCShuttleConsoleEvacuateBuiMessage());
    }
}
