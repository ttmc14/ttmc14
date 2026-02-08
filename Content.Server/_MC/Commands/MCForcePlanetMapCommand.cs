using Content.Server._MC.Xeno.Spawn;
using Content.Server.Administration;
using Content.Server.GameTicking;
using Content.Shared._RMC14.Rules;
using Content.Shared.Administration;
using Content.Shared.Prototypes;
using Robust.Shared.Console;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Server._MC.Commands;

[AdminCommand(AdminFlags.Debug)]
public sealed class MCForcePlanetMapCommand : LocalizedCommands
{
    [Dependency] private readonly IEntityManager _entity = null!;
    [Dependency] private readonly IPrototypeManager _prototype = null!;

    public override string Command => "mc_force_planet_map";
    public override string Description => "Sets the planet map to a specific planet map";
    public override string Help => "mc_force_planet_map <planetId>";

    public override void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (args.Length != 1)
        {
            shell.WriteError($"Usage: {Help}");
            return;
        }

        if (!_prototype.TryIndex<EntityPrototype>(args[0], out var planetEntity))
        {
            shell.WriteError($"No entity found with id {args[0]}");
            return;
        }

        if (!planetEntity.HasComponent<RMCPlanetMapPrototypeComponent>())
        {
            shell.WriteError($"No planet entity with {nameof(RMCPlanetMapPrototypeComponent)} found, id: {args[0]}");
            return;
        }

        if (_entity.System<GameTicker>().RunLevel != GameRunLevel.PreRoundLobby)
        {
            shell.WriteError("This command can only be run in the lobby!");
            return;
        }

        if (!_entity.System<RMCPlanetSystem>().GetAllPlanets().TryFirstOrNull(p => p.Proto.ID == planetEntity.ID, out var first))
        {
            shell.WriteError($"No planet found with id {planetEntity.ID}");
            return;
        }

        var planetSys = _entity.System<MCXenoSpawnSystem>();
        planetSys.CancelPlanetVote();
        planetSys.SetPlanet(first.Value);

        shell.WriteLine($"The next round's planet has been set to {first.Value}");
    }

    public override CompletionResult GetCompletion(IConsoleShell shell, string[] args)
    {
        if (args.Length != 1)
            return CompletionResult.Empty;

        var options = new List<CompletionOption>();
        foreach (var prototype in _prototype.EnumeratePrototypes<EntityPrototype>())
        {
            if (!prototype.HasComponent<RMCPlanetMapPrototypeComponent>())
                continue;

            options.Add(new CompletionOption(prototype.ID));
        }


        return CompletionResult.FromOptions(options);
    }
}
