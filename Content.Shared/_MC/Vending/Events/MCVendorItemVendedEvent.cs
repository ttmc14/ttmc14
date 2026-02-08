namespace Content.Shared._MC.Vending.Events;

[ByRefEvent]
public readonly struct MCVendorItemVendedEvent
{
    public readonly EntityUid Vendor;
    public readonly EntityUid Actor;
    public readonly string EntryId;
    public readonly int Amount;
    public readonly bool IsInfinite;

    public MCVendorItemVendedEvent(EntityUid vendor, EntityUid actor, string entryId, int amount, bool isInfinite)
    {
        Vendor = vendor;
        Actor = actor;
        EntryId = entryId;
        Amount = amount;
        IsInfinite = isInfinite;
    }
}
