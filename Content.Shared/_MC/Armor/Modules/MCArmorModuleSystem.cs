using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Content.Shared._MC.Armor.Modules.Components;
using Content.Shared._MC.Popup;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Interaction;
using Content.Shared.Item;
using Content.Shared.Mobs.Systems;
using Content.Shared.Movement.Systems;
using Content.Shared.Popups;
using Content.Shared.Storage;
using Content.Shared.Storage.EntitySystems;
using Content.Shared.Whitelist;
using Robust.Shared.Containers;

namespace Content.Shared._MC.Armor.Modules;

public sealed partial class MCArmorModuleSystem : EntitySystem
{
    [Dependency] private readonly SharedContainerSystem _container = null!;
    [Dependency] private readonly SharedPopupSystem _popup = null!;
    [Dependency] private readonly SharedItemSystem _item = null!;
    [Dependency] private readonly SharedHandsSystem _hands = null!;
    [Dependency] private readonly MobStateSystem _mobState = null!;
    [Dependency] private readonly MovementSpeedModifierSystem _speedModifier = null!;
    [Dependency] private readonly EntityWhitelistSystem _whitelist = null!;

    private EntityQuery<MCArmorModuleComponent> _armorModuleQuery;

    public override void Initialize()
    {
        base.Initialize();

        _armorModuleQuery = GetEntityQuery<MCArmorModuleComponent>();

        InitializeContainer();
        InitializeVerbs();

        SubscribeLocalEvent<MCArmorModularClothingComponent, InteractUsingEvent>(OnInteract, before: [ typeof(SharedStorageSystem) ]);
    }

    private void OnInteract(Entity<MCArmorModularClothingComponent> entity, ref InteractUsingEvent args)
    {
        if (!_armorModuleQuery.TryComp(args.Used, out var moduleComponent))
            return;

        if (!TryAttachModuleToAnySlot(entity, (args.Used, moduleComponent), args.User))
            return;

        args.Handled = true;
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

        return true;
    }

    private MCArmorModuleSlot? FindFreeSlotForModule(
        Entity<MCArmorModularClothingComponent> armor,
        EntityUid module)
    {
        return armor.Comp.Slots.FirstOrDefault(slot => slot.Module is null && !_whitelist.IsWhitelistFail(slot.Whitelist, module));
    }

    private bool CanAttachModule(
        Entity<MCArmorModularClothingComponent> armor,
        EntityUid module,
        EntityUid? user)
    {
        if (!_armorModuleQuery.HasComp(module))
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
        RefreshUser(user);
    }

    private void RemoveModuleEffects(
        Entity<MCArmorModularClothingComponent> entity,
        Entity<MCArmorModuleComponent> module,
        EntityUid? user)
    {
        EntityManager.RemoveComponents(entity, module.Comp.Components);
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
            if (_armorModuleQuery.TryComp(ent, out var comp))
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
        Entity<MCArmorModularClothingComponent> armor,
        Entity<MCArmorModuleComponent> module,
        EntityUid user)
    {
        if (!_container.TryRemoveFromContainer(module.Owner))
            return false;

        foreach (var slot in armor.Comp.Slots)
        {
            if (slot.Module == module.Owner)
            {
                slot.Module = null;
                break;
            }
        }

        _hands.TryPickupAnyHand(user, module);

        RefreshUser(user);
        RemoveModuleEffects(armor, module, user);
        return true;
    }
}
