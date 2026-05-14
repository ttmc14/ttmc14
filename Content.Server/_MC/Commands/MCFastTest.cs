using Content.Server.Administration;
using Content.Shared.Administration;
using Robust.Shared.Console;

namespace Content.Server._MC.Commands;

[AdminCommand(AdminFlags.Host)]
public sealed class MCFastTest : LocalizedCommands
{
    public override string Command => "mc_fast_test";

    public override void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        try
        {
            shell.ExecuteCommand("golobby");
            shell.ExecuteCommand("sudo cvar mc.round.can_end false");
            shell.ExecuteCommand("forceplanetmap MCPlanetJungleTemple");
            shell.ExecuteCommand("forcepreset MCCrash");
        }
        catch (Exception e)
        {
            shell.WriteError(e.Message);
        }
    }
}
