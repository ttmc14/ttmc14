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
        if (_player.LocalEntity is null)
        {
            shell.WriteError("Failed export, not found attached entity");
            return;
        }

        _entity.System<MCLoadoutExporterSystem>().ExportSelf();
    }
}
