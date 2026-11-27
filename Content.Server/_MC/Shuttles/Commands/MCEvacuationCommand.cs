using Content.Server.Administration;
using Content.Shared.Administration;
using Robust.Shared.Console;

namespace Content.Server._MC.Shuttles.Commands;

[AdminCommand(AdminFlags.Admin)]
public sealed class MCEvacuationCommand : LocalizedCommands
{
    [Dependency] private readonly IEntitySystemManager _systems = null!;

    public override string Command => "mc_evac";

    public override void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (args.Length != 2 || !EntityUid.TryParse(args[0], out var grid) ||
            args[1] is not ("approve" or "deny" or "silent"))
        {
            shell.WriteError(Help);
            return;
        }

        if (!_systems.GetEntitySystem<MCShuttleEvacuationSystem>().ResolveRequest(grid, args[1] == "approve", args[1] == "silent"))
        {
            shell.WriteError(Loc.GetString("mc-evac-no-request"));
        }
    }
}
