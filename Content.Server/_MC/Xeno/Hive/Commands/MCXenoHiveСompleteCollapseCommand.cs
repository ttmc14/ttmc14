using Content.Server.Administration;
using Content.Shared._MC.Xeno.Hive.Components;
using Content.Shared.Administration;
using Robust.Shared.Console;

namespace Content.Server._MC.Xeno.Hive.Commands;

[AdminCommand(AdminFlags.Debug)]
public sealed class MCXenoHiveСompleteCollapseCommand : LocalizedCommands
{
    [Dependency] private readonly IEntityManager _entityManager = null!;

    public override string Command => "mc_xeno_hive_complete_collapse";

    public override void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        var query = _entityManager.EntityQueryEnumerator<MCXenoHiveComponent>();
        while (query.MoveNext(out var uid, out var component))
        {
            foreach (var (key, _) in component.Collapse)
            {
                component.Collapse[key] = TimeSpan.Zero;
            }

            _entityManager.Dirty(uid, component);

            var meta = _entityManager.GetComponent<MetaDataComponent>(uid);
            shell.WriteLine($"Collapsed hive {meta.EntityName} ({uid})");
        }
    }
}
