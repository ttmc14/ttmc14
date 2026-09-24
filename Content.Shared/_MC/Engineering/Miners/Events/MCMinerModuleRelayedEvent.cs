namespace Content.Shared._MC.Engineering.Miners.Events;

[ByRefEvent]
public struct MCMinerModuleRelayedEvent<TEvent>(TEvent args)
{
    public TEvent Args = args;
}
