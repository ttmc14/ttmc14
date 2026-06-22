namespace Content.Shared._MC.Armor.Modules.Core.Events;

public sealed class MCArmorModuleRelayedEvent<TEvent>(TEvent args) : EntityEventArgs
{
    public TEvent Args = args;
}
