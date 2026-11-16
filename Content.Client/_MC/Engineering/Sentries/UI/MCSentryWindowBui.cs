using Content.Shared._MC.Engineering.Sentries.UI;
using Content.Shared._MC.Engineering.Sentries.UI.Message;
using JetBrains.Annotations;
using Robust.Client.UserInterface;

namespace Content.Client._MC.Engineering.Sentries.UI;

[UsedImplicitly]
public sealed class MCSentryWindowBui(EntityUid owner, Enum uiKey) : BoundUserInterface(owner, uiKey)
{
    [ViewVariables]
    private MCSentryWindow? _window;

    protected override void Open()
    {
        base.Open();

        _window = this.CreateWindow<MCSentryWindow>();
        _window.OnRadialMode += enabled => SendMessage(new MCSentryRadialModeBuiMessage(enabled));
        _window.OnAlertMode += enabled => SendMessage(new MCSentryAlertModeBuiMessage(enabled));
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        if (state is not MCSentryBuiState sentryState)
            return;

        _window?.Refresh(sentryState);
    }
}
