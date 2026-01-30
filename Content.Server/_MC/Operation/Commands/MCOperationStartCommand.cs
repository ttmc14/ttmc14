using Content.Server.Administration;
using Content.Shared._MC.Operation;
using Content.Shared.Administration;
using Robust.Shared.Console;

namespace Content.Server._MC.Operation.Commands;

[AdminCommand(AdminFlags.Debug)]
public sealed class MCOperationStartCommand : LocalizedCommands
{
    [Dependency] private readonly IEntitySystemManager _entitySystem = null!;

    public override string Command => "mc_operation_start_command";

    public override void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        _entitySystem.GetEntitySystem<MCOperationSystem>().Start();
    }
}
