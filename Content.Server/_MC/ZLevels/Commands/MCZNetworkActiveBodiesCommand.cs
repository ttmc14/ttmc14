using Content.Server._CE.ZLevels.Core;
using Content.Server.Administration;
using Content.Shared.Administration;
using Robust.Shared.Console;

namespace Content.Server._MC.ZLevels.Commands;

[AdminCommand(AdminFlags.Server | AdminFlags.Mapping)]
public sealed class MCZNetworkActiveBodiesCommand : LocalizedEntityCommands
{
    [Dependency] private readonly CEZLevelsSystem _zLevel = null!;

    public override string Command => "mc_znetwork_active_bodies";
    public override string Description => "";

    public override void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        shell.WriteLine($"Active bodies: {_zLevel.ActiveBodies.Count} (Update calls: {_zLevel.UpdateCalls})");
    }
}
