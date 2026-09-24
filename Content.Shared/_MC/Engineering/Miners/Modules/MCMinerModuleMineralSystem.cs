using Content.Shared._MC.Engineering.Miners.Components;
using Content.Shared._MC.Engineering.Miners.Components.Modules;
using Content.Shared._MC.Engineering.Miners.Events;
using Content.Shared._MC.Engineering.Miners.Events.Modules;

namespace Content.Shared._MC.Engineering.Miners.Modules;

public sealed class MCMinerModuleMineralSystem : EntitySystem
{
    [Dependency] private readonly MCMinerModuleSystem _minerModule = null!;

    public override void Initialize()
    {
        SubscribeLocalEvent<MCMinerModuleContainerComponent, MCMinerGetMineralValueEvent>(_minerModule.RelayEvent);
        SubscribeLocalEvent<MCMinerModuleMineralComponent, MCMinerModuleRelayedEvent<MCMinerGetMineralValueEvent>>(OnGetValue);
    }

    private static void OnGetValue(Entity<MCMinerModuleMineralComponent> entity, ref MCMinerModuleRelayedEvent<MCMinerGetMineralValueEvent> args)
    {
        args.Args.Add(entity.Comp.AdjustedValue);
    }
}
