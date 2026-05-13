using System.Collections.Frozen;
using System.Linq;
using Content.Shared._MC.Engineering.Vending.Components;
using Content.Shared._MC.Engineering.Vending.UI.Messages;
using Content.Shared._MC.Serialization.Loadout.Data;
using Content.Shared.Inventory;
using Content.Shared.Storage.EntitySystems;
using JetBrains.Annotations;
using Robust.Shared.Prototypes;

namespace Content.Shared._MC.Engineering.Vending;

public sealed class MCVendorQuickEquipSystem : EntitySystem
{
    private static readonly FrozenDictionary<string, int> SlotPriorities = new Dictionary<string, int>
    {
        ["jumpsuit"] = -9999,
        ["outerClothing"] = -9999,
        ["shoes"] = -9999,
    }.ToFrozenDictionary();

    [Dependency] private readonly InventorySystem _inventory = null!;
    [Dependency] private readonly SharedStorageSystem _storage = null!;
    [Dependency] private readonly MCVendingSystem _vending = null!;

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
        foreach (var slot in loadout.Slots.OrderBy(s => GetPriority(s.SlotName)))
        {
            VendSlot(targetUid, slot, vendors);
        }
    }

    [PublicAPI]
    public void VendSlot(EntityUid targetUid, MCLoadoutSlot slot, List<EntProtoId> vendors)
    {
        if (slot.Item is null)
            return;

        var itemUid = VendItem(targetUid, slot.Item, vendors);
        if (itemUid is null)
            return;

        _inventory.TryEquip(targetUid, itemUid.Value, slot.SlotName, force: true);
    }

    [PublicAPI]
    public EntityUid? VendItem(EntityUid targetUid, MCLoadoutItem item, List<EntProtoId> vendors)
    {
        var transform = Transform(targetUid);
        foreach (var vendorProto in vendors)
        {
            var vendorUid = _vending.GetVendorFirst(vendorProto, transform.MapID);
            if (vendorUid is null)
                continue;

            var spawnedUid = _vending.Vend(vendorUid.Value, item.ProtoId, transform.Coordinates, targetUid);
            if (spawnedUid is null)
                continue;

            if (item.Contains is not { Count: > 0 })
                return spawnedUid;

            foreach (var childItem in item.Contains)
            {
                var child = VendItem(targetUid, childItem, vendors);
                if (child is null)
                    continue;

                _storage.Insert(spawnedUid.Value, child.Value, out _);
            }

            return spawnedUid;
        }

        return null;
    }

    private static int GetPriority(string slot) => SlotPriorities.GetValueOrDefault(slot, 0);
}
