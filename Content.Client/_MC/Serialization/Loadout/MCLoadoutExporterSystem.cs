using System.IO;
using System.Threading.Tasks;
using Content.Shared._MC.Serialization.Loadout;
using Content.Shared._MC.Serialization.Loadout.Data;
using Robust.Client.Player;
using Robust.Client.UserInterface;

namespace Content.Client._MC.Serialization.Loadout;

public sealed class MCLoadoutExporterSystem : EntitySystem
{
    [Dependency] private readonly IFileDialogManager _fileDialog = null!;
    [Dependency] private readonly IPlayerManager _player = null!;

    [Dependency] private readonly MCLoadoutSerializerSystem _mcLoadoutSerializer = null!;

    public async Task ExportSelf()
    {
        if (_player.LocalEntity is not { } targetUid)
            return;

        await Export(_mcLoadoutSerializer.BuildEntity(targetUid));
    }

    public async Task Export(MCLoadout loadout)
    {
        var file = await _fileDialog.SaveFile(new FileDialogFilters(new FileDialogFilters.Group("yml")));
        if (file is null)
            return;

        try
        {
            var data = _mcLoadoutSerializer.LoadoutToDataNode(loadout);
            await using var writer = new StreamWriter(file.Value.fileStream);
            data.Write(writer);
        }
        finally
        {
            await file.Value.fileStream.DisposeAsync();
        }
    }

    public async Task<MCLoadout> Import()
    {
        var file = await _fileDialog.OpenFile(new FileDialogFilters(new FileDialogFilters.Group("yml")));
        if (file is null)
            throw new FileNotFoundException();

        try
        {
            return _mcLoadoutSerializer.LoadoutFromStream(file);
        }
        finally
        {
            await file.DisposeAsync();
        }
    }
}
