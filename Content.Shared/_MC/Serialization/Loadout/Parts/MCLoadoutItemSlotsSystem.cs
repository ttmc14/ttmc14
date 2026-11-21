using Content.Shared._MC.Serialization.Loadout.Data;
using Content.Shared.Containers.ItemSlots;
using Robust.Shared.Serialization;

namespace Content.Shared._MC.Serialization.Loadout.Parts;

[Serializable, NetSerializable]
public sealed partial class MCLoadoutItemSlots : MCLoadoutSlotsPart;

public sealed class MCLoadoutItemSlotsSystem : MCLoadoutSlotsSystem<ItemSlotsComponent, MCLoadoutItemSlots>
{
    [Dependency] private readonly ItemSlotsSystem _itemSlots = null!;

    protected override MCLoadoutItemSlots CreatePart() => new();

    protected override IEnumerable<Slot> GetSlots(Entity<ItemSlotsComponent> entity)
    {
        foreach (var (id, slot) in entity.Comp.Slots)
            yield return new Slot(id, slot.Item, slot.Locked);
    }

    protected override bool TryInstall(Entity<ItemSlotsComponent> entity, Slot slot, EntityUid item, EntityUid user)
    {
        var target = entity.Comp.Slots[slot.Id];
        if (!_itemSlots.CanInsert(entity, item, null, target, swap: true))
            return false;

        if (target.Item != null && !_itemSlots.TryEject(entity, target, null, out _))
            return false;

        return _itemSlots.TryInsert(entity, target, item, null);
    }

    protected override bool TryRemove(Entity<ItemSlotsComponent> entity, Slot slot, EntityUid item, EntityUid user)
        => _itemSlots.TryEject(entity, entity.Comp.Slots[slot.Id], null, out _);
}
