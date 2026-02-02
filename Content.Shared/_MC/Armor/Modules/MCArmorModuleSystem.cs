using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Content.Shared._MC.Armor.Modules.Components;
using Content.Shared._MC.Armor.Modules.Events;
using Content.Shared._MC.Popup;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Interaction;
using Content.Shared.Inventory.Events;
using Content.Shared.Item;
using Content.Shared.Mobs.Systems;
using Content.Shared.Movement.Systems;
using Content.Shared.Popups;
using Content.Shared.Storage;
using Content.Shared.Storage.EntitySystems;
using Content.Shared.Whitelist;
using Robust.Shared.Containers;
using Robust.Shared.Map.Components;

namespace Content.Shared._MC.Armor.Modules;

public abstract partial class MCArmorModuleSystem : EntitySystem
{
    [Dependency] private readonly SharedContainerSystem _container = null!;
    [Dependency] private readonly SharedPopupSystem _popup = null!;
    [Dependency] private readonly SharedItemSystem _item = null!;
    [Dependency] private readonly SharedHandsSystem _hands = null!;
    [Dependency] private readonly MobStateSystem _mobState = null!;
    [Dependency] private readonly MovementSpeedModifierSystem _speedModifier = null!;
    [Dependency] private readonly EntityWhitelistSystem _whitelist = null!;

    protected EntityQuery<MCArmorModuleComponent> ArmorModuleQuery;

    public override void Initialize()
    {
        base.Initialize();

        ArmorModuleQuery = GetEntityQuery<MCArmorModuleComponent>();

        InitializeContainer();
        InitializeVerbs();

        SubscribeLocalEvent<MCArmorModularClothingComponent, InteractUsingEvent>(OnInteract, before: [ typeof(SharedStorageSystem) ]);
        SubscribeLocalEvent<MCArmorModularClothingComponent, GotEquippedEvent>(OnArmorEquipped);
        SubscribeLocalEvent<MCArmorModularClothingComponent, GotUnequippedEvent>(OnArmorUnequipped);
    }

    private void OnInteract(Entity<MCArmorModularClothingComponent> entity, ref InteractUsingEvent args)
    {
        if (!ArmorModuleQuery.TryComp(args.Used, out var moduleComponent))
            return;

        EntityUid? user = Transform(entity).ParentUid;
        if (HasComp<MapGridComponent>(user))
            user = null;

        TryAttachModuleToAnySlot(entity, (args.Used, moduleComponent), user);
        args.Handled = true;
    }

    private void OnArmorEquipped(Entity<MCArmorModularClothingComponent> entity, ref GotEquippedEvent args)
    {
        var previous = entity.Comp.CurrentUser;

        EntityUid? user = Transform(entity).ParentUid;
        if (HasComp<MapGridComponent>(user))
            user = null;

        entity.Comp.CurrentUser = user;
        Dirty(entity);

        RaiseUserChangedOnAllModules(entity, previous, user);
    }

    private void OnArmorUnequipped(Entity<MCArmorModularClothingComponent> entity, ref GotUnequippedEvent args)
    {
        var previous = entity.Comp.CurrentUser;

        entity.Comp.CurrentUser = null;
        Dirty(entity);

        RaiseUserChangedOnAllModules(entity, previous, null);
    }

    private bool TryAttachModuleToAnySlot(
        Entity<MCArmorModularClothingComponent> entity,
        Entity<MCArmorModuleComponent> module,
        EntityUid? user)
    {
        if (!CanAttachModule(entity, module, user))
            return false;

        var slot = FindFreeSlotForModule(entity, module);
        if (slot is null)
        {
            _popup.PopupLocEntServer(user, "mc-module-no-free-slot", PopupType.MediumCaution);
            return false;
        }

        var container = _container.EnsureContainer<Container>(entity, entity.Comp.ContainerId);
        if (!_container.Insert(module.Owner, container))
            return false;

        slot.Module = module;
        ApplyModuleEffects(entity, module, user);

        var ev = new MCArmorModuleAttachedEvent(entity, module, user);
        RaiseLocalEvent(module, ref ev);

        return true;
    }

    private MCArmorModuleSlot? FindFreeSlotForModule(
        Entity<MCArmorModularClothingComponent> entity,
        EntityUid module)
    {
        return entity.Comp.Slots.FirstOrDefault(slot => slot.Module is null && _whitelist.IsWhitelistPassOrNull(slot.Whitelist, module));
    }

    private bool CanAttachModule(
        Entity<MCArmorModularClothingComponent> armor,
        EntityUid module,
        EntityUid? user)
    {
        if (!ArmorModuleQuery.HasComp(module))
            return false;

        if (!TryComp<ItemComponent>(armor, out _) ||
            !TryComp<ItemComponent>(module, out _))
            return false;

        if (IsInStorage(armor))
        {
            _popup.PopupLocEntServer(user, "mc-module-cannot-in-storage", PopupType.SmallCaution);
            return false;
        }

        return true;
    }

    private bool IsInStorage(EntityUid entity)
    {
        if (!_container.TryGetContainingContainer(entity, out var containing))
            return false;

        return TryComp<StorageComponent>(containing.Owner, out var storage) &&
               storage.StoredItems.ContainsKey(entity);
    }

    private void ApplyModuleEffects(
        Entity<MCArmorModularClothingComponent> entity,
        Entity<MCArmorModuleComponent> module,
        EntityUid? user)
    {
        EntityManager.AddComponents(entity, module.Comp.Components);

        if (user is null)
            return;

        EntityManager.AddComponents(user.Value, module.Comp.UserComponents);
        RefreshUser(user);
    }

    private void RemoveModuleEffects(
        Entity<MCArmorModularClothingComponent> entity,
        Entity<MCArmorModuleComponent> module,
        EntityUid? user)
    {
        EntityManager.RemoveComponents(entity, module.Comp.Components);

        if (user is null)
            return;

        EntityManager.RemoveComponents(user.Value, module.Comp.Components);
        RefreshUser(user);
    }

    private void RefreshUser(EntityUid? uid)
    {
        if (uid is not { } user)
            return;

        _speedModifier.RefreshMovementSpeedModifiers(user);
    }

    private IEnumerable<Entity<MCArmorModuleComponent>> EnumerateModules(
        Entity<MCArmorModularClothingComponent> entity)
    {
        if (!TryGetArmorContainer(entity, out var container))
            yield break;

        foreach (var ent in container.ContainedEntities)
        {
            if (ArmorModuleQuery.TryComp(ent, out var comp))
                yield return (ent, comp);
        }
    }

    private bool TryGetArmorContainer(Entity<MCArmorModularClothingComponent> entity, [NotNullWhen(true)] out Container? container)
    {
        container = null;
        if (!_container.TryGetContainer(entity, entity.Comp.ContainerId, out var baseContainer))
            return false;

        container = (Container) baseContainer;
        return true;
    }

    public bool HasAnyModule(Entity<MCArmorModularClothingComponent> entity)
    {
        return EnumerateModules(entity).Any();
    }

    private bool TryDetachSpecificModule(
        Entity<MCArmorModularClothingComponent> entity,
        Entity<MCArmorModuleComponent> module,
        EntityUid user)
    {
        if (!_container.TryRemoveFromContainer(module.Owner))
            return false;

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

        RefreshUser(user);
        RemoveModuleEffects(entity, module, user);
        return true;
    }

    private void RefreshEvents(Entity<MCArmorModularClothingComponent> armor)
    {
        var ev = new MCArmorModuleUserChangedEvent(null, armor.Comp.CurrentUser);
        foreach (var (entityUid, _) in EnumerateModules(armor))
        {
            RaiseLocalEvent(entityUid, ref ev);
        }
    }

    private void RaiseUserChangedOnAllModules(Entity<MCArmorModularClothingComponent> armor, EntityUid? oldUser, EntityUid? newUser)
    {
        var ev = new MCArmorModuleUserChangedEvent(oldUser, newUser);
        foreach (var (entityUid, _) in EnumerateModules(armor))
        {
            RaiseLocalEvent(entityUid, ref ev);
        }
    }
}
