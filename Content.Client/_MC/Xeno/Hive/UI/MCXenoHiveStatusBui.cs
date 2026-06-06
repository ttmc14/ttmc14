using Content.Shared._MC.Xeno.Hive.UI.Messages;
using Content.Shared._MC.Xeno.Hive.UI.Status;
using JetBrains.Annotations;
using Robust.Client.UserInterface;

namespace Content.Client._MC.Xeno.Hive.UI;

[UsedImplicitly]
public sealed class MCXenoHiveStatusBui(EntityUid owner, Enum uiKey) : BoundUserInterface(owner, uiKey)
{
    [ViewVariables] private MCXenoHiveStatusWindow? _window;

    protected override void Open()
    {
        base.Open();

        _window = this.CreateWindow<MCXenoHiveStatusWindow>();
        _window.OpenCentered();

        _window.OnButtonEvolve += () => SendMessage(new MCXenoHiveStatusEvolutionMessage());
        _window.OnButtonDevolve += () => SendMessage(new MCXenoHiveStatusDevolveMessage());
        _window.OnButtonSkins += () => SendMessage(new MCXenoHiveStatusSkinMessage());
        _window.OnButtonBlessings += () => SendMessage(new MCXenoHiveStatusBlessingsMessage());
        _window.OnButtonWatch += entity => SendMessage(new MCXenoHiveStatusWatchMessage(entity));
    }

    protected override void UpdateState(BoundUserInterfaceState uncasted)
    {
        if (uncasted is not MCXenoHiveStatusXenosBuiState state)
            return;

        _window?.SetState(state);
    }
}
