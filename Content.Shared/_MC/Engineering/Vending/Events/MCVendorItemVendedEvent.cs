namespace Content.Shared._MC.Engineering.Vending.Events;

[ByRefEvent]
public readonly struct MCVendorItemVendedEvent(EntityUid vendorUid, EntityUid itemUid)
{
    public readonly EntityUid VendorUid = vendorUid;
    public readonly EntityUid ItemUid = itemUid;
}
