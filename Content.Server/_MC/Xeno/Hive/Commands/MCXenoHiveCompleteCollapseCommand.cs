using Content.Server.Administration;
using Content.Shared._MC.Xeno.Hive.Components;
using Content.Shared.Administration;
using Robust.Shared.Console;

namespace Content.Server._MC.Xeno.Hive.Commands;

[AdminCommand(AdminFlags.Debug)]
public sealed class MCXenoHiveCompleteCollapseCommand : LocalizedCommands
{
    private const string CommandName = "mc_hive_complete_collapse";

    [Dependency] private readonly IEntityManager _entityManager = null!;

    public override string Command => CommandName;
    public override string Help => $"{CommandName} <hiveUid>";

    public override void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (args.Length != 1)
        {
            shell.WriteError(Loc.GetString("shell-need-between-arguments", ("lower", 1), ("upper", 1)));
            return;
        }

        if (!NetEntity.TryParse(args[0], out var entityUidNet) || !_entityManager.TryGetEntity(entityUidNet, out var entityUid))
        {
            shell.WriteError(Loc.GetString("cmd-parse-failure-uid", ("arg", args[0])));
            return;
        }

        if (!_entityManager.TryGetComponent<MCXenoHiveComponent>(entityUid, out var component))
        {
            shell.WriteError(Loc.GetString("shell-entity-with-uid-lacks-component", ("uid", entityUid), ("componentName", nameof(MCXenoHiveComponent))));
            return;
        }

        foreach (var (key, _) in component.Collapse)
            component.Collapse[key] = TimeSpan.Zero;

        shell.WriteLine($"Collapsed hive {entityUid}");
    }

    public override CompletionResult GetCompletion(IConsoleShell shell, string[] args) =>
        args.Length != 1
            ? CompletionResult.Empty
            : CompletionResult.FromHintOptions(CompletionHelper.Components<MCXenoHiveComponent>(args[0]), string.Empty);
}
