using System.Linq;
using Content.Shared._MC.Armor.Modules.Core.Components;
using Content.Shared._MC.Serialization.Loadout.Data;
using Robust.Shared.Containers;

namespace Content.Shared._MC.Serialization.Loadout;

public sealed class MCArmorModularClothingLoadoutModule : IMCLoadoutModule
{
    private readonly IEntityManager _entityManager = null!;
    private readonly SharedContainerSystem _container = null!;

    public MCArmorModularClothingLoadoutModule(IEntityManager entityManager, SharedContainerSystem container)
    {
        _entityManager = entityManager;
        _container = container;
    }

    public List<MCLoadoutSlot>? TryBuildSlots(EntityUid targetUid, Func<EntityUid, MCLoadoutItem?> buildItem)
    {
        if (!_entityManager.TryGetComponent(targetUid, out MCArmorModularClothingComponent? clothing))
            return null;

        var slots = new List<MCLoadoutSlot>();
        foreach (var moduleSlot in clothing.Slots)
        {
            var moduleUid = moduleSlot.Module;
            if (!moduleUid.HasValue)
                continue;

            var item = buildItem(moduleUid.Value);
            if (item is null)
                continue;

            slots.Add(new MCLoadoutSlot
            {
                SlotName = moduleSlot.Id,
                Item = item,
            });
        }

        return slots;
    }

    public bool TryApplySlot(EntityUid targetUid, MCLoadoutSlot slot, EntityUid itemUid)
    {
        if (slot.Item is null)
            return false;

        var moduleProtoId = slot.Item.ProtoId;
        if (string.IsNullOrEmpty(moduleProtoId))
            return false;

        if (!_entityManager.TryGetComponent(targetUid, out MCArmorModularClothingComponent? clothing))
            return false;

        if (!_entityManager.TryGetComponent(itemUid, out MCArmorModuleComponent? module))
            return false;

        var moduleSlot = clothing.Slots.FirstOrDefault(s => s.Id == slot.SlotName);
        if (moduleSlot is null || moduleSlot.Module.HasValue)
            return false;

        var container = _container.EnsureContainer<Container>(targetUid, clothing.ContainerId);

        // Removed CanInsert check. Insert() automatically handles removing the item
        // from the vendor's temporary storage/container, preventing it from failing and dropping on the floor.
        if (!_container.Insert(itemUid, container))
            return false;

        moduleSlot.Module = itemUid;
        _entityManager.AddComponents(targetUid, module.Components);

        return true;
    }
}
