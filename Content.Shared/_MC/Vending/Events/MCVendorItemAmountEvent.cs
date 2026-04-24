namespace Content.Shared._MC.Vending.Events;

[ByRefEvent]
public readonly struct MCVendorItemAmountEvent(
    EntityUid vendor,
    EntityUid actor,
    string entryId,
    int amount,
    bool isInfinite)
{
    public readonly EntityUid Vendor = vendor;
    public readonly EntityUid Actor = actor;
    public readonly string EntryId = entryId;
    public readonly int Amount = amount;
    public readonly bool IsInfinite = isInfinite;
}
