using Content.Client._MC.Serialization.Loadout;
using Content.Shared._MC.Serialization.Loadout;
using Content.Shared._MC.Vending.UI.Messages;
using JetBrains.Annotations;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;

namespace Content.Client._MC.Vending.UI;

[UsedImplicitly]
public sealed class MCVendorQuickEquipBui(EntityUid owner, Enum uiKey) : BoundUserInterface(owner, uiKey)
{
    [Dependency] private readonly IEntityManager _entities = null!;

    private MCLoadoutSerializerSystem _mcLoadoutSerializer = null!;
    private MCLoadoutExporterSystem _mcLoadoutExporter = null!;

    [ViewVariables]
    private MCVendorQuickEquip? _window;

    protected override void Open()
    {
        base.Open();

        _mcLoadoutSerializer = _entities.System<MCLoadoutSerializerSystem>();
        _mcLoadoutExporter = _entities.System<MCLoadoutExporterSystem>();

        _window = this.CreateWindow<MCVendorQuickEquip>();

        _window.ExportButton.OnPressed += OnExportPressed;
        _window.ImportButton.OnPressed += OnImportPressed;
    }

    private void OnExportPressed(BaseButton.ButtonEventArgs obj)
    {
        _ = _mcLoadoutExporter.ExportSelf();
    }

    private void OnImportPressed(BaseButton.ButtonEventArgs obj)
    {
        _mcLoadoutExporter.Import().ContinueWith(action =>
        {
            if (action.IsFaulted || action.IsCanceled)
                return;

            var result = action.GetAwaiter().GetResult();
            SendMessage(new MCVendorQuickEquipVendMessage(result));
        });
    }
}
