using Content.Shared._MC.Armor.Modules.Core.Components;
using Content.Shared._MC.Armor.Modules.Core.Events;
using Content.Shared.DoAfter;
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
using JetBrains.Annotations;
using Robust.Shared.Containers;
using Robust.Shared.Map.Components;

namespace Content.Shared._MC.Armor.Modules.Core;

public abstract partial class MCArmorModuleSharedSystem : EntitySystem
{
    [Dependency] private readonly SharedDoAfterSystem _doAfter = null!;
    [Dependency] private readonly SharedContainerSystem _container = null!;
    [Dependency] private readonly SharedPopupSystem _popup = null!;
    [Dependency] private readonly SharedItemSystem _item = null!;
    [Dependency] private readonly SharedHandsSystem _hands = null!;
    [Dependency] private readonly MobStateSystem _mobState = null!;
    [Dependency] private readonly MovementSpeedModifierSystem _speedModifier = null!;
    [Dependency] private readonly EntityWhitelistSystem _whitelist = null!;
    [Dependency] private readonly SharedStorageSystem _storage = null!;

    [PublicAPI] protected EntityQuery<ItemComponent> ItemQuery;
    [PublicAPI] protected EntityQuery<MapGridComponent> MapGridQuery;
    [PublicAPI] protected EntityQuery<StorageComponent> StorageQuery;
    [PublicAPI] protected EntityQuery<MCArmorModuleComponent> ArmorModuleQuery;
    [PublicAPI] protected EntityQuery<MCArmorModularClothingComponent> ArmorModularClothingQuery;

    public override void Initialize()
    {
        // Queries
        ItemQuery = GetEntityQuery<ItemComponent>();
        MapGridQuery = GetEntityQuery<MapGridComponent>();
        StorageQuery = GetEntityQuery<StorageComponent>();

        ArmorModuleQuery = GetEntityQuery<MCArmorModuleComponent>();
        ArmorModularClothingQuery = GetEntityQuery<MCArmorModularClothingComponent>();

        InitializeContainer();
        InitializeVerbs();

        SubscribeLocalEvent<MCArmorModularClothingComponent, InteractUsingEvent>(OnInteract, before: [ typeof(SharedStorageSystem) ]);

        SubscribeLocalEvent<MCArmorModularClothingComponent, MCArmorModuleAttachDoAfterEvent>(OnArmorAttachDoAfter);
        SubscribeLocalEvent<MCArmorModularClothingComponent, MCArmorModuleDeattachDoAfterEvent>(OnArmorDeattachDoAfter);

        SubscribeLocalEvent<MCArmorModularClothingComponent, GotEquippedEvent>(OnArmorEquipped);
        SubscribeLocalEvent<MCArmorModularClothingComponent, GotUnequippedEvent>(OnArmorUnequipped);
    }

    private void OnInteract(Entity<MCArmorModularClothingComponent> entity, ref InteractUsingEvent args)
    {
        if (!ArmorModuleQuery.TryComp(args.Used, out var moduleComponent))
            return;

        args.Handled = true;

        var user = Transform(entity).ParentUid;
        if (MapGridQuery.HasComp(user))
        {
            TryAttachModuleToAnySlot(entity, (args.Used, moduleComponent));
            return;
        }

        TryAttachModuleToAnySlot(entity, (args.Used, moduleComponent), user, duration: moduleComponent.DurationEquip);
    }

    private void OnArmorAttachDoAfter(Entity<MCArmorModularClothingComponent> entity, ref MCArmorModuleAttachDoAfterEvent args)
    {
        if (args.Handled || args.Cancelled || args.Used is null)
            return;

        if (!ArmorModuleQuery.TryComp(args.Used, out var moduleComponent))
            return;

        var slot = FindFreeSlotForModule(entity, args.Used.Value);
        if (slot is null)
            return;

        AttachModuleToAnySlot(entity, (args.Used.Value, moduleComponent), args.User, slot);
    }

    private void OnArmorDeattachDoAfter(Entity<MCArmorModularClothingComponent> entity, ref MCArmorModuleDeattachDoAfterEvent args)
    {
        if (args.Handled || args.Cancelled || args.Used is null)
            return;

        if (!ArmorModuleQuery.TryComp(args.Used, out var moduleComponent))
            return;

        DetachSpecificModule(entity, (args.Used.Value, moduleComponent), args.User);
    }

    private void OnArmorEquipped(Entity<MCArmorModularClothingComponent> entity, ref GotEquippedEvent args)
    {
        var previous = entity.Comp.CurrentUser;

        EntityUid? user = Transform(entity).ParentUid;
        if (MapGridQuery.HasComp(user))
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


    private void ApplyModuleEffects(
        Entity<MCArmorModularClothingComponent> entity,
        Entity<MCArmorModuleComponent> module,
        EntityUid? user)
    {
        EntityManager.AddComponents(entity, module.Comp.Components);

        if (user is null)
            return;

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
}
