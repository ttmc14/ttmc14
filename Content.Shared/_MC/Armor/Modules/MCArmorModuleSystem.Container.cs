using System.Linq;
using Content.Shared._MC.Armor.Modules.Components;
using Robust.Shared.Containers;

namespace Content.Shared._MC.Armor.Modules;

public partial class MCArmorModuleSystem
{
    private void InitializeContainer()
    {
        SubscribeLocalEvent<MCArmorModularClothingComponent, EntInsertedIntoContainerMessage>(OnInserted);
        SubscribeLocalEvent<MCArmorModularClothingComponent, EntRemovedFromContainerMessage>(OnRemoved);
    }

    private void OnInserted(Entity<MCArmorModularClothingComponent> entity, ref EntInsertedIntoContainerMessage args)
    {
        if (entity.Comp.ContainerId != args.Container.ID)
            return;

        if (!TryInsertModule(entity, args.Entity))
        {
            _container.Remove(args.Entity, args.Container, force: true);
            return;
        }
    }

    private void OnRemoved(Entity<MCArmorModularClothingComponent> entity, ref EntRemovedFromContainerMessage args)
    {
        if (entity.Comp.ContainerId != args.Container.ID)
            return;

        TryRemoveModule(entity, args.Entity);
    }

    #region Module Logic

    private bool TryInsertModule(Entity<MCArmorModularClothingComponent> entity, EntityUid moduleUid)
    {
        var slot = FindSlotModule(entity, moduleUid);
        if (slot == null)
            return false;

        slot.Module = moduleUid;

        ContainerRefresh(entity);
        return true;
    }

    private bool TryRemoveModule(Entity<MCArmorModularClothingComponent> entity, EntityUid uid)
    {
        var slot = entity.Comp.Slots.FirstOrDefault(s => s.Module == uid);
        if (slot is null)
            return false;

        slot.Module = null;

        ContainerRefresh(entity);
        return true;
    }

    private MCArmorModuleSlot? FindSlotModule(Entity<MCArmorModularClothingComponent> entity, EntityUid uid)
    {
        return entity.Comp.Slots.FirstOrDefault(s => s.Module is null && _whitelist.IsWhitelistPassOrNull(s.Whitelist, uid));
    }

    private void ContainerRefresh(Entity<MCArmorModularClothingComponent> entity)
    {
        Dirty(entity);
        _item.VisualsChanged(entity);
    }

    #endregion
}
