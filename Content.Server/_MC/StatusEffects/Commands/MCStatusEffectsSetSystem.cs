using Content.Server.Administration;
using Content.Server.StatusEffectNew;
using Content.Shared.Administration;
using Robust.Shared.Console;
using Robust.Shared.Prototypes;

namespace Content.Server._MC.StatusEffects.Commands;

[AdminCommand(AdminFlags.Debug)]
public sealed class MCStatusEffectsSetSystem : LocalizedCommands
{
    private const string Name = "mc_status_effects_set";

    [Dependency] private readonly IEntityManager _entity = null!;
    [Dependency] private readonly IPrototypeManager _prototype = null!;

    public override string Command => Name;
    public override string Help => $"{Command} <uid> <protoId> <time>";

    public override void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        var system = _entity.System<StatusEffectsSystem>();

        if (args.Length < 3)
        {
            shell.WriteError($"Usage: {Help}");
            return;
        }

        if (!EntityUid.TryParse(args[0], out var entityUid))
        {
            shell.WriteError("Invalid uid");
            return;
        }

        var protoId = args[1];
        if (!_prototype.HasIndex(protoId))
        {
            shell.WriteError("Invalid protoId");
            return;
        }

        if (!float.TryParse(args[2], out var duration))
        {
            shell.WriteError("Invalid time");
            return;
        }

        system.TrySetTime(entityUid, protoId, TimeSpan.FromSeconds(duration));
    }
}
