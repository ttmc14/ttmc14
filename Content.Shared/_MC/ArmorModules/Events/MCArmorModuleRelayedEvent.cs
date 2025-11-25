namespace Content.Shared._MC.ArmorModules.Events;

public sealed class MCArmorModuleRelayedEvent<TEvent> : EntityEventArgs
{
    public TEvent Args;

    public MCArmorModuleRelayedEvent(TEvent args)
    {
        Args = args;
    }
}
