using System.Linq;
using Content.Server.Administration;
using Content.Shared._RMC14.Rules;
using Content.Shared.Administration;
using Content.Shared.Prototypes;
using Robust.Shared.Console;
using Robust.Shared.Prototypes;

namespace Content.Server._MC.Commands;

[AdminCommand(AdminFlags.Host)]
public sealed class MCFastMapTestCommand : LocalizedCommands
{
    [Dependency] private readonly IPrototypeManager _prototype = null!;

    public override string Command => "mc_fast_map_test";

    public override void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        var maps = (from prototype in _prototype.EnumeratePrototypes<EntityPrototype>()
            where prototype.HasComponent<RMCPlanetMapPrototypeComponent>()
            select prototype.ID).ToList();

        try
        {
            shell.ExecuteCommand("sudo cvar mc.round.can_end false");

            foreach (var map in maps)
            {
                shell.ExecuteCommand("golobby");
                shell.ExecuteCommand($"forceplanetmap {map}");
                shell.ExecuteCommand("forcepreset MCCrash");
            }
        }
        catch (Exception e)
        {
            shell.WriteError(e.Message);
        }
    }
}
