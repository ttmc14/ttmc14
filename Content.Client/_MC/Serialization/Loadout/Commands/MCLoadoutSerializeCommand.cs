using Content.Shared._MC.Serialization.Loadout;
using Robust.Client.Player;
using Robust.Shared.Console;

namespace Content.Client._MC.Serialization.Loadout.Commands;

public sealed class MCLoadoutSerializeCommand : LocalizedCommands
{
    [Dependency] private readonly IPlayerManager _player = null!;
    [Dependency] private readonly IEntityManager _entity = null!;

    public override string Command => "mc_loadout_export";
    public override string Help => "mc_loadout_export";

    public override void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (_player.LocalEntity is not { } targetUid)
        {
            shell.WriteError("Failed export, not found attached entity");
            return;
        }

        var loadoutSerializer = _entity.System<MCLoadoutSerializerSystem>();
        var loadoutExporter = _entity.System<MCLoadoutExporterSystem>();

        var loadout  = loadoutSerializer.BuildEntity(targetUid);

        loadoutExporter.Export(loadout).ContinueWith(task =>
        {
            if (task.IsFaulted)
            {
                // Fucking sa-a-a-a-andbox!
                // [ERRO] res.typecheck: Sandbox violation: Access to type not allowed: [System.Runtime]System.AggregateException

                shell.WriteError($"Export failed, idk why, robust sandbox sucks");
                return;
            }

            shell.WriteLine("Loadout successfully exported!");
        });
    }
}
