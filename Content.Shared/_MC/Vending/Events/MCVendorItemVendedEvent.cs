namespace Content.Shared._MC.Vending.Events;

[ByRefEvent]
public readonly struct MCVendorItemVendedEvent(EntityUid vendorUid, EntityUid itemUid)
{
    public readonly EntityUid VendorUid = vendorUid;
    public readonly EntityUid ItemUid = itemUid;
}
