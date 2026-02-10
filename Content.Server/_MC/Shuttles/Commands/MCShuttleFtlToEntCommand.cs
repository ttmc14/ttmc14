using Content.Server.Administration;
using Content.Server.Shuttles.Components;
using Content.Server.Shuttles.Systems;
using Content.Shared.Administration;
using Content.Shared.Coordinates;
using Robust.Shared.Console;

namespace Content.Server._MC.Shuttles.Commands;

[AdminCommand(AdminFlags.Debug)]
public sealed class MCShuttleFtlToEntCommand : LocalizedCommands
{
    [Dependency] private readonly IEntitySystemManager _entitySystem = null!;
    [Dependency] private readonly IEntityManager _entManager = null!;

    public override string Command => "mc_shuttle_ftl_to_ent";
    public override string Help => "mc_shuttle_ftl_to_ent <shuttleUid> <targetUid> [angleDeg]";

    public override void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (args.Length < 2)
        {
            shell.WriteError($"Usage: {Help}");
            return;
        }

        if (!EntityUid.TryParse(args[0], out var shuttleUid))
        {
            shell.WriteError("Invalid shuttleUid.");
            return;
        }

        if (!EntityUid.TryParse(args[1], out var targetUid))
        {
            shell.WriteError("Invalid coordinates or mapId.");
            return;
        }

        var angle = Angle.Zero;
        if (args.Length >= 3 && float.TryParse(args[2], out var angleDeg))
            angle = Angle.FromDegrees(angleDeg);

        if (!_entManager.TryGetComponent<ShuttleComponent>(shuttleUid, out var shuttleComp))
        {
            shell.WriteError($"Entity {shuttleUid} does not have ShuttleComponent.");
            return;
        }

        var coords = targetUid.ToCoordinates();
        var shuttleSystem = _entitySystem.GetEntitySystem<ShuttleSystem>();

        shuttleSystem.FTLToCoordinates(
            shuttleUid,
            shuttleComp,
            coords,
            angle
        );

        shell.WriteLine($"FTL jump started for shuttle {shuttleUid} to {coords} with angle {angle}.");
    }
}
