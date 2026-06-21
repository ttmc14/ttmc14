using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Content.Shared._MC.Armor.Modules.Core.Components;
using Content.Shared._MC.Armor.Modules.Core.Events;
using Content.Shared._MC.Popup;
using Content.Shared.DoAfter;
using Content.Shared.Item;
using Content.Shared.Popups;
using JetBrains.Annotations;
using Robust.Shared.Containers;

namespace Content.Shared._MC.Armor.Modules.Core;

public abstract partial class MCArmorModuleSharedSystem
{
    [PublicAPI]
    public bool TryAttachModuleToAnySlot(
        Entity<MCArmorModularClothingComponent> entity,
        Entity<MCArmorModuleComponent> module,
        EntityUid? user = null,
        TimeSpan? duration = null)
    {
        duration ??= TimeSpan.Zero;

        if (!CanAttachModule(entity, module, user))
            return false;

        var slot = FindFreeSlotForModule(entity, module);
        if (slot is null)
        {
            _popup.PopupLocEntServer(user, "mc-module-no-free-slot", PopupType.MediumCaution);
            return false;
        }

        var container = _container.EnsureContainer<Container>(entity, entity.Comp.ContainerId);
        if (!_container.CanInsert(module.Owner, container))
            return false;

        if (duration == TimeSpan.Zero || user is null)
        {
            AttachModuleToAnySlot(entity, module, user, slot);
            return true;
        }

        var ev = new MCArmorModuleAttachDoAfterEvent();
        return _doAfter.TryStartDoAfter(new DoAfterArgs(EntityManager, user.Value, module.Comp.DurationEquip, ev, entity, user, module)
        {
            BlockDuplicate = true,
            BreakOnDropItem = true,
            BreakOnMove = true,
            BreakOnHandChange = true,
        });
    }

    [PublicAPI]
    public void AttachModuleToAnySlot(
        Entity<MCArmorModularClothingComponent> entity,
        Entity<MCArmorModuleComponent> module,
        EntityUid? user,
        MCArmorModuleSlot slot)
    {
        var container = _container.EnsureContainer<Container>(entity, entity.Comp.ContainerId);
        if (!_container.Insert(module.Owner, container))
            return;

        slot.Module = module;
        ApplyModuleEffects(entity, module, user);

        _doTransfer.Add((entity, module));

        var ev = new MCArmorModuleAttachedEvent(entity, module, user);
        RaiseLocalEvent(module, ref ev);
    }

    [PublicAPI]
    public bool TryDetachSpecificModule(
        Entity<MCArmorModularClothingComponent> entity,
        Entity<MCArmorModuleComponent> module,
        EntityUid user,
        TimeSpan? duration = null)
    {
        duration ??= TimeSpan.Zero;

        if (duration == TimeSpan.Zero)
        {
            DetachSpecificModule(entity, module, user);
            return true;
        }

        var ev = new MCArmorModuleDeattachDoAfterEvent();
        return _doAfter.TryStartDoAfter(new DoAfterArgs(EntityManager, user, module.Comp.DurationEquip, ev, entity, user, module)
        {
            BlockDuplicate = true,
            BreakOnDropItem = true,
            BreakOnMove = true,
            BreakOnHandChange = true,
        });
    }

    [PublicAPI]
    public void DetachSpecificModule(
        Entity<MCArmorModularClothingComponent> entity,
        Entity<MCArmorModuleComponent> module,
        EntityUid user)
    {
        if (!_container.TryRemoveFromContainer(module.Owner))
            return;

        foreach (var slot in entity.Comp.Slots)
        {
            if (slot.Module != module.Owner)
                continue;

            slot.Module = null;
            break;
        }

        _hands.TryPickupAnyHand(user, module);

        var ev = new MCArmorModuleDetachedEvent(entity, module, user);
        RaiseLocalEvent(module, ref ev);

        if (StorageQuery.TryComp(entity, out var storage))
        {
            foreach (var stored in storage.Container.ContainedEntities.ToArray())
            {
                _storage.Insert(module, stored, out _, playSound: false);
            }
        }

        RefreshUser(user);
        RemoveModuleEffects(entity, module, user);
    }

    [PublicAPI]
    public bool CanAttachModule(
        Entity<MCArmorModularClothingComponent> armor,
        EntityUid module,
        EntityUid? user)
    {
        if (!ArmorModuleQuery.HasComp(module))
            return false;

        if (!TryComp<ItemComponent>(armor, out _) ||
            !TryComp<ItemComponent>(module, out _))
            return false;

        if (!IsInStorage(armor))
            return true;

        _popup.PopupLocEntServer(user, "mc-module-cannot-in-storage", PopupType.SmallCaution);
        return false;
    }

    [PublicAPI]
    public bool TryGetArmorContainer(
        Entity<MCArmorModularClothingComponent> entity,
        [NotNullWhen(true)] out Container? container)
    {
        container = null;
        if (!_container.TryGetContainer(entity, entity.Comp.ContainerId, out var baseContainer))
            return false;

        container = (Container) baseContainer;
        return true;
    }

    [PublicAPI]
    public EntityUid? GetUser(EntityUid uid)
    {
        var parent = Transform(uid).ParentUid;
        return ArmorModularClothingQuery.TryComp(parent, out var containerComponent)
            ? containerComponent.CurrentUser
            : null;
    }

    [PublicAPI]
    public Container GetContainer(Entity<MCArmorModularClothingComponent> entity, EntityUid moduleUid)
    {
        return _container.EnsureContainer<Container>(entity, entity.Comp.ContainerId);
    }

    [PublicAPI]
    public bool HasAnyModule(Entity<MCArmorModularClothingComponent> entity)
    {
        return EnumerateModules(entity).Any();
    }

    [PublicAPI]
    public bool IsInStorage(EntityUid entity)
    {
        if (!_container.TryGetContainingContainer(entity, out var containing))
            return false;

        return StorageQuery.TryComp(containing.Owner, out var storage) &&
               storage.StoredItems.ContainsKey(entity);
    }

    [PublicAPI]
    public MCArmorModuleSlot? FindFreeSlotForModule(
        Entity<MCArmorModularClothingComponent> entity,
        EntityUid module)
    {
        return entity.Comp.Slots.FirstOrDefault(slot =>
            slot.Module is null &&
            _whitelist.IsWhitelistPassOrNull(slot.Whitelist, module)
        );
    }

    [PublicAPI]
    public void RefreshUser(EntityUid? uid)
    {
        if (uid is not { } user)
            return;

        _speedModifier.RefreshMovementSpeedModifiers(user);
    }

    [PublicAPI]
    public void RefreshEvents(Entity<MCArmorModularClothingComponent> entity)
    {
        var ev = new MCArmorModuleUserChangedEvent(null, entity.Comp.CurrentUser);
        foreach (var (entityUid, _) in EnumerateModules(entity))
        {
            RaiseLocalEvent(entityUid, ref ev);
        }
    }

    [PublicAPI]
    public void RaiseUserChangedOnAllModules(Entity<MCArmorModularClothingComponent> entity, EntityUid? oldUser, EntityUid? newUser)
    {
        var ev = new MCArmorModuleUserChangedEvent(oldUser, newUser);
        foreach (var (entityUid, _) in EnumerateModules(entity))
        {
            RaiseLocalEvent(entityUid, ref ev);
        }
    }
}
