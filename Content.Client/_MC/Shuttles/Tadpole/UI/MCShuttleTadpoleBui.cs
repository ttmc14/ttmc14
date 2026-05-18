using Content.Shared._MC.Shuttles.Tadpole.UI;
using JetBrains.Annotations;
using Robust.Client.UserInterface;

namespace Content.Client._MC.Shuttles.Tadpole.UI;

[UsedImplicitly]
public sealed class MCShuttleTadpoleBui(EntityUid owner, Enum uiKey) : BoundUserInterface(owner, uiKey)
{
    [ViewVariables]
    private MCShuttleTadpoleWindow? _window;

    protected override void Open()
    {
        base.Open();

        _window = this.CreateWindow<MCShuttleTadpoleWindow>();
        _window.Open();

        _window.ButtonLand.OnPressed += _ => SendMessage(new MCShuttleTadpoleLandBuiMessage());
        _window.ButtonReturn.OnPressed += _ => SendMessage(new MCShuttleTadpoleReturnBuiMessage());
        _window.ButtonTakOff.OnPressed += _ => SendMessage(new MCShuttleTadpoleTakeOffBuiMessage());
    }
}
