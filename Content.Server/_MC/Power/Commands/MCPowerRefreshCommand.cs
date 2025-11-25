using Content.Server._RMC14.Power;
using Content.Server.Administration;
using Content.Shared.Administration;
using Robust.Shared.Console;

namespace Content.Server._MC.Power.Commands;

[AdminCommand(AdminFlags.Debug)]
public sealed class MCPowerRefreshCommand : LocalizedCommands
{
    [Dependency] private readonly IEntitySystemManager _entitySystem = default!;

    public override string Command => "mc_power_refresh";

    public override void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        _entitySystem.GetEntitySystem<RMCPowerSystem>().RecalculatePower();
    }
}
