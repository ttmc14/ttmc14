using Content.Server.Administration;
using Content.Server.Shuttles.Components;
using Content.Server.Shuttles.Systems;
using Content.Shared.Administration;
using Content.Shared.Coordinates;
using Content.Shared.Shuttles.Components;
using Content.Shared.Timing;
using Robust.Shared.Console;

namespace Content.Server._MC.Shuttles.Commands;

[AdminCommand(AdminFlags.Debug)]
public sealed class MCShuttleFtlLandNowCommand : LocalizedCommands
{
    [Dependency] private readonly IEntityManager _entManager = null!;

    public override string Command => "mc_shuttle_land_now";
    public override string Description => "mc_shuttle_land_now <shuttleUid>";

    public override void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (args.Length < 1)
        {
            shell.WriteError($"Usage: {Description}");
            return;
        }

        if (!EntityUid.TryParse(args[0], out var shuttleUid))
        {
            shell.WriteError("Invalid shuttleUid.");
            return;
        }
        if (!_entManager.TryGetComponent<FTLComponent>(shuttleUid, out var ftlComponent))
        {
            shell.WriteError($"Entity {shuttleUid} does not have {nameof(FTLComponent)}.");
            return;
        }

        ftlComponent.StateTime = new StartEndTime(ftlComponent.StateTime.Start, TimeSpan.FromSeconds(1));
        shell.WriteLine($"FTL land forced for shuttle {shuttleUid}.");
    }

    public override CompletionResult GetCompletion(IConsoleShell shell, string[] args)
    {
        if (args.Length != 1)
            return CompletionResult.Empty;

        return CompletionResult.FromHintOptions(CompletionHelper.Components<FTLComponent>(args[0]), string.Empty);
    }
}
