using Content.Server.Administration;
using Content.Shared._MC.Xeno.QueueJoin;
using Content.Shared.Administration;
using Robust.Shared.Console;

namespace Content.Server._MC.Xeno.Hive.Commands;

[AdminCommand(AdminFlags.Debug)]
public sealed class MCXenoHiveQueueJoinSingletonCommand : LocalizedCommands
{
    private const string CommandName = "mc_hive_queue_join_singleton";

    [Dependency] private readonly IEntityManager _entityManager = null!;

    public override string Command => CommandName;
    public override string Help => CommandName;

    public override void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (args.Length != 0)
        {
            shell.WriteError(Loc.GetString("shell-need-between-arguments", ("lower", 0), ("upper", 0)));
            return;
        }

        shell.ExecuteCommand($"vv { _entityManager.System<MCXenoHiveQueueJoinSystem>().NetEntityContainer}");
    }
}
