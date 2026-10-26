using Content.Shared._MC.Serialization.Loadout.Data;
using Content.Shared._RMC14.Attachable.Components;
using Robust.Shared.Containers;

namespace Content.Shared._MC.Serialization.Loadout;

public sealed class AttachableHolderLoadoutModule : IMCLoadoutModule
{
    private readonly IEntityManager _entityManager = null!;
    private readonly SharedContainerSystem _container = null!;

    public AttachableHolderLoadoutModule(IEntityManager entityManager, SharedContainerSystem container)
    {
        _entityManager = entityManager;
        _container = container;
    }

    public List<MCLoadoutSlot>? TryBuildSlots(EntityUid targetUid, Func<EntityUid, MCLoadoutItem?> buildItem)
    {
        if (!_entityManager.TryGetComponent(targetUid, out AttachableHolderComponent? holder))
            return null;

        var slots = new List<MCLoadoutSlot>();

        // Iterate over the actual container slots to save currently attached items
        foreach (var slotId in holder.Slots.Keys)
        {
            if (!_container.TryGetContainer(targetUid, slotId, out var container) || container.ContainedEntities.Count == 0)
                continue;

            var attachableUid = container.ContainedEntities[0];
            var item = buildItem(attachableUid);
            if (item is null)
                continue;

            slots.Add(new MCLoadoutSlot
            {
                SlotName = slotId,
                Item = item,
            });
        }

        return slots;
    }

    public bool TryApplySlot(EntityUid targetUid, MCLoadoutSlot slot, EntityUid itemUid)
    {
        if (slot.Item is null)
            return false;

        var attachableProtoId = slot.Item.ProtoId;
        if (string.IsNullOrEmpty(attachableProtoId))
            return false;

        if (!_entityManager.TryGetComponent(targetUid, out AttachableHolderComponent? holder))
            return false;

        var attachableContainer = _container.EnsureContainer<ContainerSlot>(targetUid, slot.SlotName);
        if (!_container.Insert(itemUid, attachableContainer))
            return false;

        return true;
    }
}
