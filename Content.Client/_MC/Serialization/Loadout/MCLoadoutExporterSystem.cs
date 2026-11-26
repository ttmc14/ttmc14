using System.IO;
using System.Threading.Tasks;
using Content.Shared._MC.Serialization.Loadout;
using Content.Shared._MC.Serialization.Loadout.Data;
using Robust.Client.Player;
using Robust.Client.UserInterface;

namespace Content.Client._MC.Serialization.Loadout;

public sealed class MCLoadoutExporterSystem : EntitySystem
{
    private static readonly FileDialogFilters LoadoutFiles = new(new FileDialogFilters.Group("yml"));

    [Dependency] private readonly IFileDialogManager _fileDialog = null!;
    [Dependency] private readonly IPlayerManager _player = null!;

    [Dependency] private readonly MCLoadoutSerializerSystem _serializer = null!;

    public override void Initialize()
    {
        SubscribeNetworkEvent<MCLoadoutExportResponse>(OnExportResponse);
    }

    public void ExportSelf()
    {
        if (_player.LocalEntity != null)
            RaiseNetworkEvent(new MCLoadoutExportRequest());
    }

    private async void OnExportResponse(MCLoadoutExportResponse message)
    {
        try
        {
            await Export(message.Loadout);
        }
        catch (Exception e)
        {
            Log.Error($"Loadout export failed: {e.Message}");
        }
    }

    public async Task Export(MCLoadout loadout)
    {
        if (await _fileDialog.SaveFile(LoadoutFiles) is not { } file)
            return;

        await using var writer = new StreamWriter(file.fileStream);
        _serializer.LoadoutToDataNode(loadout).Write(writer);
    }

    public async Task<MCLoadout?> Import()
    {
        await using var file = await _fileDialog.OpenFile(LoadoutFiles);
        return file == null ? null : _serializer.LoadoutFromStream(file);
    }
}
