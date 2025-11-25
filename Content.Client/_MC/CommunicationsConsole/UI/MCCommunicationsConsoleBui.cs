using Content.Shared._MC.CommunicationsConsole.UI;
using JetBrains.Annotations;
using Robust.Client.UserInterface;

namespace Content.Client._MC.CommunicationsConsole.UI;

[UsedImplicitly]
public sealed class MCCommunicationsConsoleBui : BoundUserInterface
{
    [Dependency] private readonly IEntityManager _entities = default!;

    [ViewVariables]
    private MCCommunicationsConsoleWindow? _window;

    public MCCommunicationsConsoleBui(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
    }

    protected override void Open()
    {
        base.Open();

        _window = this.CreateWindow<MCCommunicationsConsoleWindow>();
        _window.ERTCallButton.OnPressed += _ => SendMessage(new MCCommunicationsConsoleERTCallBuiMessage());
    }
}
