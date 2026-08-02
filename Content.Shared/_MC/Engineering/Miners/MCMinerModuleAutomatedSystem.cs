using Content.Shared._MC.Engineering.Miners.Components;
using Content.Shared._MC.Engineering.Miners.Events;

namespace Content.Shared._MC.Engineering.Miners;

public sealed class MCMinerModuleAutomatedSystem : EntitySystem
{
    [Dependency] private readonly MCMinerModuleSystem _minerModule = null!;

    public override void Initialize()
    {
        SubscribeLocalEvent<MCMinerModuleContainerComponent, MCMinerModuleAutomatedEvent>(_minerModule.RelayEvent);
        SubscribeLocalEvent<MCMinerModuleAutomatedComponent, MCMinerModuleRelayedEvent<MCMinerModuleAutomatedEvent>>(OnAutomated);
    }

    private static void OnAutomated(Entity<MCMinerModuleAutomatedComponent> entity, ref MCMinerModuleRelayedEvent<MCMinerModuleAutomatedEvent> args)
    {
        args.Args.Automated = true;
    }
}
