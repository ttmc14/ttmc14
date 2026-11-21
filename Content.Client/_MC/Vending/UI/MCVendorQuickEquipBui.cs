using System.Threading.Tasks;
using Content.Client._MC.Serialization.Loadout;
using Content.Shared._MC.Engineering.Vending.UI.Messages;
using JetBrains.Annotations;
using Robust.Client.UserInterface;

namespace Content.Client._MC.Vending.UI;

[UsedImplicitly]
public sealed class MCVendorQuickEquipBui(EntityUid owner, Enum uiKey) : BoundUserInterface(owner, uiKey)
{
    protected override void Open()
    {
        base.Open();

        var exporter = EntMan.System<MCLoadoutExporterSystem>();
        var window = this.CreateWindow<MCVendorQuickEquip>();
        window.OnVend += loadout => SendMessage(new MCVendorQuickEquipVendMessage(loadout));
        window.ExportButton.OnPressed += _ => exporter.ExportSelf();
        window.ImportButton.OnPressed += async _ => await ImportLoadout(window, exporter);
    }

    private static async Task ImportLoadout(MCVendorQuickEquip window, MCLoadoutExporterSystem exporter)
    {
        window.ImportButton.Disabled = true;
        try
        {
            if (await exporter.Import() is { } loadout && window.IsOpen)
                window.ShowLoadout(loadout);
        }
        catch (Exception e)
        {
            Logger.ErrorS("mc.loadout", $"Loadout import failed: {e.Message}");
            if (window.IsOpen)
                window.ShowLoadout(null);
        }
        finally
        {
            if (window.IsOpen)
                window.ImportButton.Disabled = false;
        }
    }
}
