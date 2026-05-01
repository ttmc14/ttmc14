using Content.Client._MC.ASRS.UI.Views;
using Content.Client._MC.Beacon.UI;
using Content.Shared._MC.ASRS.Components;
using JetBrains.Annotations;
using Robust.Client.UserInterface;

namespace Content.Client._MC.ASRS.UI;

[UsedImplicitly]
public sealed partial class MCASRSBui(EntityUid owner, Enum uiKey) : BoundUserInterface(owner, uiKey)
{
    [Dependency] private readonly IEntityManager _entity = null!;

    [ViewVariables]
    private MCASRSWindow _window = null!;

    [ViewVariables]
    private MCBeaconChooseWindow _beaconChooseWindow = null!;

    public bool SettingShowIcons { get; private set; } = true;

    private MCASRSView? _openedView;

    protected override void Open()
    {
        base.Open();

        _window = this.CreateWindow<MCASRSWindow>();
        _beaconChooseWindow = new MCBeaconChooseWindow();
        _beaconChooseWindow.Selected += OnSelected;

        if (!_entity.TryGetComponent<MCASRSConsoleComponent>(Owner, out var component))
            return;

        InitializeStore();

        InitializeViewCategory(component);
        InitializeViewPendingOrders();
        InitializeViewRequests();
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        _beaconChooseWindow.Close();
        _openedView = null;
    }

    private void OpenView(MCASRSView view)
    {
        _window.OpenView(this, view);
        _openedView = view;
    }
}
