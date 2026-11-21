using System.Linq;
using Content.Shared._MC.Engineering.Vending.Components;
using Content.Shared._MC.Engineering.Vending.UI.Messages;
using Content.Shared._MC.Serialization.Loadout;
using Content.Shared._MC.Serialization.Loadout.Data;
using Content.Shared.Inventory;
using JetBrains.Annotations;
using Robust.Shared.Prototypes;

namespace Content.Shared._MC.Engineering.Vending;

public sealed class MCVendorQuickEquipSystem : EntitySystem
{
    [Dependency] private readonly InventorySystem _inventory = null!;
    [Dependency] private readonly MCVendingSystem _vending = null!;
    [Dependency] private readonly MCLoadoutSerializerSystem _serializer = null!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MCVendorQuickEquipComponent, MCVendorQuickEquipVendMessage>(OnVendMessage);
    }

    private void OnVendMessage(Entity<MCVendorQuickEquipComponent> entity, ref MCVendorQuickEquipVendMessage args)
    {
        VendLoadout(args.Actor, args.Loadout, entity.Comp.Vendors);
    }

    [PublicAPI]
    public void VendLoadout(EntityUid targetUid, MCLoadout loadout, List<EntProtoId> vendors)
    {
        foreach (var slot in loadout.Slots.OrderByDescending(slot => slot.SlotName is "jumpsuit" or "outerClothing" or "shoes"))
        {
            VendSlot(targetUid, slot, vendors);
        }
    }

    [PublicAPI]
    public void VendSlot(EntityUid targetUid, MCLoadoutSlot slot, List<EntProtoId> vendors)
    {
        if (slot.Item is null)
            return;

        if (VendItem(targetUid, slot.Item, vendors) is { } itemUid)
            _inventory.TryEquip(targetUid, itemUid, slot.SlotName, force: true);
    }

    [PublicAPI]
    public EntityUid? VendItem(EntityUid targetUid, MCLoadoutItem item, List<EntProtoId> vendors)
    {
        var context = new MCLoadoutRestoreContext(_serializer, targetUid, proto => Purchase(targetUid, proto, vendors));
        return context.Vend(item);
    }

    private EntityUid? Purchase(EntityUid targetUid, EntProtoId prototype, List<EntProtoId> vendors)
    {
        var transform = Transform(targetUid);
        foreach (var vendorProto in vendors)
        {
            if (_vending.GetVendorFirst(vendorProto, transform.MapID) is not { } vendorUid)
                continue;

            if (_vending.Vend(vendorUid, prototype, transform.Coordinates, targetUid) is { } itemUid)
                return itemUid;
        }

        return null;
    }
}
