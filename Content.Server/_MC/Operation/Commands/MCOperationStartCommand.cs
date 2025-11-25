using Content.Shared._MC.Operation;
using Robust.Shared.Console;

namespace Content.Server._MC.Operation.Commands;

public sealed class MCOperationStartCommand : LocalizedCommands
{
    [Dependency] private readonly IEntitySystemManager _entitySystem = default!;

    public override string Command => "mc_operation_start_command";

    public override void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        _entitySystem.GetEntitySystem<MCOperationSystem>().Start();
    }
}
