using Content.Server.Administration;
using Content.Shared._MC.Serialization.Loadout;
using Content.Shared.Administration;
using Robust.Shared.Console;

namespace Content.Server._MC.Serialization.Loadout.Commands;

[AdminCommand(AdminFlags.Debug)]
public sealed class MCLoadoutSerializeCommand  : LocalizedCommands
{
    [Dependency] private readonly IEntityManager _entity = null!;

    public override string Command => "mc_loadout_serialize";
    public override string Help => "mc_loadout_serialize <targetUid>";

    public override void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (args.Length < 1)
        {
            shell.WriteError($"Usage: {Help}");
            return;
        }

        if (!EntityUid.TryParse(args[0], out var targetUid))
        {
            shell.WriteError("Invalid targetUid.");
            return;
        }

        var loadoutSerializer = _entity.System<MCLoadoutSerializerSystem>();
        var data  = loadoutSerializer.BuildEntity(targetUid);

        shell.WriteLine($"Data: {data}");
    }

    public override CompletionResult GetCompletion(IConsoleShell shell, string[] args)
    {
        return args.Length != 1
            ? CompletionResult.Empty
            : CompletionResult.FromHintOptions(CompletionHelper.NetEntities(args[0], _entity), string.Empty);
    }
}
