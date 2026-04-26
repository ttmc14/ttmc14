using System.Linq;
using Content.Server.Administration;
using Content.Shared._MC.Xeno.Hive.Components;
using Content.Shared._RMC14.Xenonids;
using Content.Shared.Administration;
using Robust.Shared.Toolshed;

namespace Content.Server._MC.Xeno.Hive.Commands;

[ToolshedCommand(Name = "mc_hive"), AdminCommand(AdminFlags.Admin)]
public sealed class MCXenoHiveCommand : ToolshedCommand
{
    [CommandImplementation("alldefault")]
    public void AllDefault([CommandInvocationContext] IInvocationContext ctx)
    {
        EntityUid firstHive = default;

        var hives = EntityManager.EntityQueryEnumerator<MCXenoHiveComponent>();
        while (hives.MoveNext(out var uid, out _))
        {
            if (firstHive == default || uid.Id < firstHive.Id)
                firstHive = uid;
        }

        if (firstHive == default)
        {
            ctx.WriteLine("No hives were found.");
            return;
        }

        var amount = 0;

        var xenos = EntityManager.EntityQueryEnumerator<MCXenoHiveComponent>();
        var hiveSystem = EntityManager.System<MCXenoHiveSystem>();
        while (xenos.MoveNext(out var uid, out _))
        {
            if (hiveSystem.HasHive(uid))
                continue;

            hiveSystem.SetHive(uid, firstHive);
            amount++;
        }

        var friendly = EntityManager.EntityQueryEnumerator<MCXenoHiveComponent>();
        while (friendly.MoveNext(out var uid, out _))
        {
            if (hiveSystem.HasHive(uid))
                continue;

            hiveSystem.SetHive(uid, firstHive);
            amount++;
        }

        ctx.WriteLine($"Set the hive of {amount} rogue xenos to {firstHive}.");
    }

    [CommandImplementation("set")]
    public EntityUid Set(
        [CommandInvocationContext] IInvocationContext context,
        [PipedArgument] EntityUid target,
        [CommandArgument] Entity<MCXenoHiveComponent> hive)
    {
        if (!HasComp<MCXenoHiveComponent>(target) && !HasComp<XenoFriendlyComponent>(target))
        {
            context.WriteLine($"Entity {target} does not have {nameof(MCXenoHiveComponent)} or {nameof(XenoFriendlyComponent)}");
            return target;
        }

        Sys<MCXenoHiveSystem>().SetHive(target, hive);
        return target;
    }

    [CommandImplementation("set")]
    public IEnumerable<EntityUid> Set(
        [CommandInvocationContext] IInvocationContext context,
        [PipedArgument] IEnumerable<EntityUid> targets,
        [CommandArgument] Entity<MCXenoHiveComponent> hive)
    {
        return targets.Select(xeno => Set(context, xeno, hive));
    }

    [CommandImplementation("settargeted")]
    public IEnumerable<EntityUid> SetTargeted(
        [CommandInvocationContext] IInvocationContext context,
        [CommandArgument] IEnumerable<EntityUid> targets,
        [CommandArgument] Entity<MCXenoHiveComponent> hive)
    {
        return targets.Select(xeno => Set(context, xeno, hive));
    }
}
