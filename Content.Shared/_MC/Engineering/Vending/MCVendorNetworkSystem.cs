using Content.Shared._MC.Engineering.Vending.Components;
using Content.Shared._MC.Engineering.Vending.Events;
using Content.Shared._RMC14.Vendors;

namespace Content.Shared._MC.Engineering.Vending;

public sealed class MCVendorNetworkSystem : EntitySystem
{
    public override void Initialize()
    {
        SubscribeLocalEvent<MCVendorNetworkComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<MCVendorItemAmountEvent>(OnItemVended);
    }

    public void OnMapInit(Entity<MCVendorNetworkComponent> entity, ref MapInitEvent args)
    {
        if (!TryComp<CMAutomatedVendorComponent>(entity, out var vendorComponent))
            return;

        foreach (var section in vendorComponent.Sections)
        {
            foreach (var entry in section.Entries)
            {
                if (entry.Amount is null)
                    continue;

                if (!entity.Comp.SharedAmounts.TryGetValue(entry.Id, out var current))
                {
                    entity.Comp.SharedAmounts[entry.Id] = entry.Amount.Value;
                    continue;
                }

                entity.Comp.SharedAmounts[entry.Id] = int.Min(current, entry.Amount.Value);
            }
        }

        Dirty(entity);
    }

    private void OnItemVended(ref MCVendorItemAmountEvent ev)
    {
        if (ev.IsInfinite || !TryComp<MCVendorNetworkComponent>(ev.Vendor, out var netComponent))
            return;

        var current = netComponent.SharedAmounts.GetValueOrDefault(ev.EntryId, int.MaxValue);

        current -= ev.Amount;
        if (current < 0)
            current = 0;

        netComponent.SharedAmounts[ev.EntryId] = current;
        Dirty(ev.Vendor, netComponent);

        var enumerator = EntityQueryEnumerator<MCVendorNetworkComponent, CMAutomatedVendorComponent>();
        while (enumerator.MoveNext(out var uid, out var otherNet, out var vendor))
        {
            if (otherNet.NetworkId != netComponent.NetworkId)
                continue;

            ApplySharedAmountToVendor(uid, vendor, ev.EntryId, current);
        }
    }

    private void ApplySharedAmountToVendor(
        EntityUid vendorUid,
        CMAutomatedVendorComponent vendor,
        string entryId,
        int sharedAmount)
    {
        foreach (var section in vendor.Sections)
        {
            foreach (var entry in section.Entries)
            {
                if (entry.Id != entryId)
                    continue;

                entry.Amount = sharedAmount;
                Dirty(vendorUid, vendor);
                break;
            }
        }
    }
}
