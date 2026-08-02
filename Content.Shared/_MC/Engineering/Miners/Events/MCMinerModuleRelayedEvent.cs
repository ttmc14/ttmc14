namespace Content.Shared._MC.Engineering.Miners.Events;

public sealed class MCMinerModuleRelayedEvent<TEvent>(TEvent args) : EntityEventArgs
{
    public TEvent Args = args;
}
