using System.Linq;
using Content.Server.Administration;
using Content.Shared._MC.Xeno.Hive.Components;
using Content.Shared._MC.Xeno.Hive.Systems;
using Content.Shared._MC.Xeno.Hive.Systems.Main;
using Content.Shared.Administration;
using Robust.Shared.Console;

namespace Content.Server._MC.Xeno.Hive.Commands;

[AdminCommand(AdminFlags.Debug)]
public sealed class MCXenoHiveSlotsCommand : LocalizedCommands
{
    [Dependency] private readonly IEntitySystemManager _entitySystem = null!;
    [Dependency] private readonly IEntityManager _entityManager = null!;

    public override string Command => "mc_xeno_hive_slots";

    public override void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        var hiveSystem = _entitySystem.GetEntitySystem<MCSharedXenoHiveSystem>();

        var query = _entityManager.EntityQueryEnumerator<MCXenoHiveComponent>();
        while (query.MoveNext(out var uid, out var component))
        {
            var meta = _entityManager.GetComponent<MetaDataComponent>(uid);

            var tierSlots = hiveSystem.GetTierSlots((uid, component));
            var freeslots = hiveSystem.GetAvailableTierSlots((uid, component));

            var slotsStr = string.Join(", ", tierSlots.Select(kv => $"Tier {kv.Key}: {kv.Value}"));
            var slotsFreeStr = string.Join(", ", freeslots.Select(kv => $"Tier {kv.Key}: {kv.Value}"));

            shell.WriteLine($"Hive {meta.EntityName} ({uid}):\n - Total: {slotsStr}\n - Free: {slotsFreeStr}");
        }
    }
}
