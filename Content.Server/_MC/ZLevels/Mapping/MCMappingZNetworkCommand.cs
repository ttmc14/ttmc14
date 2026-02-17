using Content.Server._CE.ZLevels.Core;
using Content.Server.Administration;
using Content.Server.GameTicking;
using Content.Shared._MC.Map;
using Content.Shared._RMC14.Rules;
using Content.Shared.Administration;
using Content.Shared.Prototypes;
using Robust.Server.GameObjects;
using Robust.Shared.Console;
using Robust.Shared.EntitySerialization;
using Robust.Shared.EntitySerialization.Systems;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Server._MC.ZLevels.Mapping;

[AdminCommand(AdminFlags.Server | AdminFlags.Mapping)]
public sealed class MCMappingZNetworkCommand : LocalizedEntityCommands
{
    [Dependency] private readonly IPrototypeManager _proto = null!;
    [Dependency] private readonly MapLoaderSystem _mapLoader = null!;
    [Dependency] private readonly CEZLevelsSystem _zLevel = null!;
    [Dependency] private readonly MetaDataSystem _meta = null!;
    [Dependency] private readonly MapSystem _map = null!;
    [Dependency] private readonly IComponentFactory _componentFactory = null!;

    public override string Command => "mc_planet_znetwork_mapping";
    public override string Description => "Load existed planet map prototype as ZNetwork for mapping";

    public override CompletionResult GetCompletion(IConsoleShell shell, string[] args)
    {
        var options = new List<CompletionOption>();
        foreach (var map in _proto.EnumeratePrototypes<EntityPrototype>())
        {
            if (!map.HasComponent<MCPlanetMapPrototypeComponent>())
                continue;

            options.Add(new CompletionOption(map.ID, map.ID));
        }

        return CompletionResult.FromHintOptions(options, $"Entities with {nameof(MCPlanetMapPrototypeComponent)}");
    }

    public override void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (shell.Player is not { } player)
        {
            shell.WriteError(Loc.GetString("shell-cannot-run-command-from-server"));
            return;
        }

        if (args.Length != 1)
        {
            shell.WriteError("Wrong arguments count.");
            return;
        }

        if (!_proto.TryIndex<EntityPrototype>(args[0], out var entity))
        {
            shell.WriteError($"Unknown entity {args[0]}");
            return;
        }

        var rmcPlanetComponentName = _componentFactory.GetComponentName<RMCPlanetMapPrototypeComponent>();
        if (!entity.TryGetComponent<RMCPlanetMapPrototypeComponent>(rmcPlanetComponentName, out var rmcPlanetComponent))
        {
            shell.WriteError("Can't find RMCPlanetMapPrototypeComponent");
            return;
        }

        var mcPlanetComponentName = _componentFactory.GetComponentName<MCPlanetMapPrototypeComponent>();
        if (!entity.TryGetComponent<MCPlanetMapPrototypeComponent>(mcPlanetComponentName, out var mcPlanetComponent))
        {
            shell.WriteError("Can't find MCPlanetMapPrototypeComponent");
            return;
        }

        var network = _zLevel.CreateZNetwork();
        _meta.SetEntityName(network, $"Mapping zNetwork: {entity.Name}");
        Dictionary<EntityUid, int> dict = new();

        List<MapId> createdMaps = new();

        var opts = new DeserializationOptions {StoreYamlUids = true};

        //Load default map
        if (!_mapLoader.TryLoadMap(rmcPlanetComponent.Map, out var defaultMapEnt, out _, opts))
        {
            shell.WriteError($"Failed to load default zNetwork map: {rmcPlanetComponent.Map.ToString()}!");
            return;
        }

        dict.Add(defaultMapEnt.Value, 0);
        createdMaps.Add(defaultMapEnt.Value.Comp.MapId);
        _meta.SetEntityName(defaultMapEnt.Value, $"Mapping {entity.Name}");

        //Loading maps below first
        var depth = mcPlanetComponent.MapsBelow.Count * -1;
        foreach (var path in mcPlanetComponent.MapsBelow)
        {
            if (!_mapLoader.TryLoadMap(path, out var mapEnt, out _, opts))
            {
                shell.WriteError($"Failed to load zNetwork map (depth {depth}): {path.ToString()}!");
                return;
            }

            dict.Add(mapEnt.Value, depth);
            createdMaps.Add(mapEnt.Value.Comp.MapId);
            _meta.SetEntityName(mapEnt.Value, $"Mapping {entity.Name} [{depth}]");
            depth++;
        }

        depth = 1;
        foreach (var path in mcPlanetComponent.MapsAbove)
        {
            if (!_mapLoader.TryLoadMap(path, out var mapEnt, out _, opts))
            {
                shell.WriteError($"Failed to load zNetwork map (depth {depth}): {path.ToString()}!");
                return;
            }

            dict.Add(mapEnt.Value, depth);
            createdMaps.Add(mapEnt.Value.Comp.MapId);
            _meta.SetEntityName(mapEnt.Value, $"Mapping {entity.Name} [{depth}]");
            depth++;
        }

        //Was the maps actually created or did it fail somehow?
        var success = true;
        foreach (var mapId in createdMaps)
        {
            if (!_map.MapExists(mapId))
            {
                success = false;
                shell.WriteError($"For some reason some maps dont exist after loading! MapId: {mapId}");
            }
        }

        if (!_zLevel.TryAddMapsIntoZNetwork(network, dict))
        {
            shell.WriteError($"Failed to create zNetwork from loaded maps!");
            return;
        }

        if (!success)
        {
            foreach (var mapId in createdMaps)
            {
                _map.DeleteMap(mapId);
            }
            shell.WriteError("Unloading all created maps...");
            return;
        }

        //Maps successfully created. run misc helpful mapping commands
        if (player.AttachedEntity is { Valid: true } playerEntity &&
            (EntityManager.GetComponent<MetaDataComponent>(playerEntity).EntityPrototype is not { } proto || proto != GameTicker.AdminObserverPrototypeName))
        {
            shell.ExecuteCommand("aghost");
        }

        // don't interrupt mapping with events or auto-shuttle
        shell.ExecuteCommand("changecvar events.enabled false");
        shell.ExecuteCommand("changecvar shuttle.auto_call_time 0");

        //TODO: Autosaves

        shell.ExecuteCommand($"tp 0 0 {defaultMapEnt.Value.Comp.MapId}");
        shell.RemoteExecuteCommand("mappingclientsidesetup");
        foreach (var mapId in createdMaps)
        {
            DebugTools.Assert(_map.IsPaused(mapId));
        }
    }
}
