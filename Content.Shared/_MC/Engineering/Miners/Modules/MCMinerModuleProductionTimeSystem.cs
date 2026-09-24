using Content.Shared._MC.Engineering.Miners.Components;
using Content.Shared._MC.Engineering.Miners.Components.Modules;
using Content.Shared._MC.Engineering.Miners.Events;
using Content.Shared._MC.Engineering.Miners.Events.Modules;

namespace Content.Shared._MC.Engineering.Miners.Modules;

public sealed class MCMinerModuleProductionTimeSystem : EntitySystem
{
    [Dependency] private readonly MCMinerModuleSystem _minerModule = null!;

    public override void Initialize()
    {
        SubscribeLocalEvent<MCMinerModuleContainerComponent, MCMinerGetProductionTimeEvent>(_minerModule.RelayEvent);
        SubscribeLocalEvent<MCMinerModuleMinerProductionTimeComponent, MCMinerModuleRelayedEvent<MCMinerGetProductionTimeEvent>>(OnGetValue);
    }

    private static void OnGetValue(Entity<MCMinerModuleMinerProductionTimeComponent> entity, ref MCMinerModuleRelayedEvent<MCMinerGetProductionTimeEvent> args)
    {
        if (entity.Comp.ReplaceTime is not { } replaceTime)
            return;

        args.Args.Set(replaceTime);
    }
}
