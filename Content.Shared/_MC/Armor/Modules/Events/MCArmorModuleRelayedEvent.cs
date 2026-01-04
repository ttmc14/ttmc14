namespace Content.Shared._MC.Armor.Modules.Events;

public sealed class MCArmorModuleRelayedEvent<TEvent> : EntityEventArgs
{
    public TEvent Args;

    public MCArmorModuleRelayedEvent(TEvent args)
    {
        Args = args;
    }
}
