using Content.Server.Administration;
using Content.Shared._MC.Xeno.Hive.Components;
using Content.Shared.Administration;
using Robust.Shared.Console;

namespace Content.Server._MC.Xeno.Hive.Commands;

[AdminCommand(AdminFlags.Debug)]
public sealed class MCXenoHiveBurrowedAddCommand : LocalizedCommands
{
    private const string CommandName = "mc_hive_burrowed_add";

    [Dependency] private readonly IEntityManager _entityManager = null!;

    public override string Command => CommandName;
    public override string Help => $"{CommandName} <hiveUid> [amount]";

    public override void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (args.Length is 0 or > 2)
        {
            shell.WriteError(Loc.GetString("shell-need-between-arguments", ("lower", 1), ("upper", 2)));
            return;
        }

        if (!NetEntity.TryParse(args[0], out var entityUidNet) || !_entityManager.TryGetEntity(entityUidNet, out var entityUid))
        {
            shell.WriteError(Loc.GetString("cmd-parse-failure-uid", ("arg", args[0])));
            return;
        }

        var amount = 1;
        if (args.Length > 1 && !int.TryParse(args[1], out amount))
        {
            shell.WriteError(Loc.GetString("cmd-parse-failure-integer", ("arg", args[1])));
            return;
        }

        if (!_entityManager.TryGetComponent<MCXenoHiveComponent>(entityUid, out var component))
        {
            shell.WriteError(Loc.GetString("shell-entity-with-uid-lacks-component", ("uid", entityUid), ("componentName", nameof(MCXenoHiveComponent))));
            return;
        }

        _entityManager.System<MCXenoHiveSystem>().AddBurrowedLarva((entityUid.Value, component), amount);
    }

    public override CompletionResult GetCompletion(IConsoleShell shell, string[] args) =>
        args.Length != 1
            ? CompletionResult.Empty
            : CompletionResult.FromHintOptions(CompletionHelper.Components<MCXenoHiveComponent>(args[0]), string.Empty);
}
