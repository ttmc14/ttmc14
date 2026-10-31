using Content.Server.Administration;
using Content.Shared._MC.Xeno.Hive.Components;
using Content.Shared._RMC14.Xenonids;
using Content.Shared.Administration;
using Robust.Shared.Console;

namespace Content.Server._MC.Xeno.Hive.Commands;

[AdminCommand(AdminFlags.Debug)]
public sealed class MCXenoHiveAllCommand : LocalizedCommands
{
    private const string CommandName = "mc_hive_all";

    [Dependency] private readonly IEntityManager _entityManager = null!;

    public override string Command => CommandName;
    public override string Help => $"{CommandName} [hiveUid]";

    public override void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (args.Length > 1)
        {
            shell.WriteError(Loc.GetString("shell-need-between-arguments", ("lower", 0), ("upper", 1)));
            return;
        }

        var system = _entityManager.System<MCXenoHiveSystem>();

        var entityUid = system.DefaultHive;
        if (args.Length >= 1)
        {
            if (!NetEntity.TryParse(args[0], out var entityUidNet) || !_entityManager.TryGetEntity(entityUidNet, out var remoteEntityUid))
            {
                shell.WriteError(Loc.GetString("cmd-parse-failure-uid", ("arg", args[0])));
                return;
            }

            entityUid = remoteEntityUid;
        }

        var query = _entityManager.EntityQueryEnumerator<XenoFriendlyComponent>();
        while (query.MoveNext(out var uid, out _))
            system.SetHive(uid, entityUid);
    }

    public override CompletionResult GetCompletion(IConsoleShell shell, string[] args) =>
        args.Length != 1
            ? CompletionResult.Empty
            : CompletionResult.FromHintOptions(CompletionHelper.Components<MCXenoHiveComponent>(args[0]), string.Empty);
}
