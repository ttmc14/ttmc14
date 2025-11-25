using Content.Shared._MC.Shuttle.Console.UI;
using JetBrains.Annotations;
using Robust.Client.UserInterface;

namespace Content.Client._MC.Shuttle.Console.UI;

[UsedImplicitly]
public sealed class MCShuttleConsoleBui : BoundUserInterface
{
    [ViewVariables]
    private MCShuttleConsoleWindow? _window;

    public MCShuttleConsoleBui(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
    }

    protected override void Open()
    {
        base.Open();

        _window = this.CreateWindow<MCShuttleConsoleWindow>();
        _window.EvacuationButton.OnPressed += _ => SendMessage(new MCShuttleConsoleEvacuateBuiMessage());
    }
}
