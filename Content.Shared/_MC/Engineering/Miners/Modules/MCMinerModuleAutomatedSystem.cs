using Content.Shared._MC.Engineering.Miners.Components;
using Content.Shared._MC.Engineering.Miners.Components.Modules;
using Content.Shared._MC.Engineering.Miners.Events;
using Content.Shared._MC.Engineering.Miners.Events.Modules;

namespace Content.Shared._MC.Engineering.Miners.Modules;

public sealed class MCMinerModuleAutomatedSystem : EntitySystem
{
    [Dependency] private readonly MCMinerModuleSystem _minerModule = null!;

    public override void Initialize()
    {
        SubscribeLocalEvent<MCMinerModuleContainerComponent, MCMinerGetAutomatedEvent>(_minerModule.RelayEvent);
        SubscribeLocalEvent<MCMinerModuleAutomatedComponent, MCMinerModuleRelayedEvent<MCMinerGetAutomatedEvent>>(OnAutomated);
    }

    private static void OnAutomated(Entity<MCMinerModuleAutomatedComponent> entity,
        ref MCMinerModuleRelayedEvent<MCMinerGetAutomatedEvent> args)
    {
        args.Args.Automated = true;
    }
}
