using Content.Shared._MC.Armor;
using Content.Shared._MC.ArmorModules.Events;
using Content.Shared._RMC14.Webbing;
using Content.Shared._RMC14.Xenonids;
using Content.Shared.Clothing;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Interaction;
using Content.Shared.Inventory;
using Content.Shared.Item;
using Content.Shared.Mobs.Systems;
using Content.Shared.Movement.Components;
using Content.Shared.Movement.Systems;
using Content.Shared.Popups;
using Content.Shared.Storage;
using Content.Shared.Verbs;
using Robust.Shared.Containers;

namespace Content.Shared._MC.ArmorModules;

public sealed class MCArmorModuleSystem : EntitySystem
{
    [Dependency] private readonly SharedContainerSystem _container = null!;
    [Dependency] private readonly SharedPopupSystem _popup = null!;
    [Dependency] private readonly SharedItemSystem _item = null!;
    [Dependency] private readonly SharedHandsSystem _hands = null!;
    [Dependency] private readonly MobStateSystem _mobState = null!;
    [Dependency] private readonly MovementSpeedModifierSystem _speedModifier = null!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MCArmorModularClothingComponent, EntInsertedIntoContainerMessage>(OnClothingInserted);
        SubscribeLocalEvent<MCArmorModularClothingComponent, EntRemovedFromContainerMessage>(OnClothingRemoved);

        SubscribeLocalEvent<MCArmorModularClothingComponent, InteractUsingEvent>(OnInteract);
        SubscribeLocalEvent<MCArmorModularClothingComponent, GetVerbsEvent<EquipmentVerb>>(OnGetVerbs);
        SubscribeLocalEvent<MCArmorModularClothingComponent, GetVerbsEvent<InteractionVerb>>(OnGetVerbsInteraction);

        SubscribeLocalEvent<MCArmorModularClothingComponent, InventoryRelayedEvent<RefreshMovementSpeedModifiersEvent>>(RelayEvent);
        SubscribeLocalEvent<MCArmorModularClothingComponent, InventoryRelayedEvent<MCArmorGetEvent>>(RelayEvent);

        SubscribeLocalEvent<MCArmorComponent, MCArmorModuleRelayedEvent<MCArmorGetEvent>>(OnModuleGetRelayed);
        SubscribeLocalEvent<ClothingSpeedModifierComponent, MCArmorModuleRelayedEvent<RefreshMovementSpeedModifiersEvent>>(OnModuleMovementSpeedModifier);
    }

    private void OnClothingInserted(Entity<MCArmorModularClothingComponent> clothing, ref EntInsertedIntoContainerMessage args)
    {
        if (clothing.Comp.Container != args.Container.ID)
            return;

        clothing.Comp.ModuleUid = args.Entity;

        Dirty(clothing);
        _item.VisualsChanged(clothing);
    }

    private void OnClothingRemoved(Entity<MCArmorModularClothingComponent> clothing, ref EntRemovedFromContainerMessage args)
    {
        if (clothing.Comp.Container != args.Container.ID)
            return;

        clothing.Comp.ModuleUid = null;

        Dirty(clothing);
        _item.VisualsChanged(clothing);
    }

    private void OnInteract(Entity<MCArmorModularClothingComponent> entity, ref InteractUsingEvent args)
    {
        if (TryGetModule((entity, entity), out _))
            return;

        Attach(entity, args.Used, args.User, out var handled);
        args.Handled = handled;
    }

    private void OnGetVerbs(Entity<MCArmorModularClothingComponent> entity, ref GetVerbsEvent<EquipmentVerb> args)
    {
        if (!args.CanAccess || !args.CanInteract || HasComp<XenoComponent>(args.User))
            return;

        if (!TryGetModule((entity, entity), out var module))
            return;

        var wearer = Transform(entity).ParentUid;
        var user = args.User;

        // To avoid duplicate verbs
        if (user == wearer)
            return;

        // To prevent stripping webbing from alive players
        if (!_mobState.IsDead(wearer))
            return;

        args.Verbs.Add(new EquipmentVerb
        {
            Text = Loc.GetString("mc-armor-remove-module"),
            Act = () => Detach(entity, user),
            IconEntity = GetNetEntity(entity),
        });
    }

    private void OnGetVerbsInteraction(Entity<MCArmorModularClothingComponent> entity, ref GetVerbsEvent<InteractionVerb> args)
    {
        if (!args.CanAccess || !args.CanInteract || HasComp<XenoComponent>(args.User))
            return;

        if (!TryGetModule((entity, entity), out var module))
            return;

        var user = args.User;
        args.Verbs.Add(new InteractionVerb
        {
            Text = Loc.GetString("mc-armor-remove-module"),
            Act = () => Detach(entity, user),
            IconEntity = GetNetEntity(entity),
        });
    }

    public bool Attach(Entity<MCArmorModularClothingComponent> entity, EntityUid moduleUid, EntityUid? userUid, out bool handled)
    {
        handled = false;

        if (!TryComp<MCArmorModuleComponent>(moduleUid, out var module))
            return false;

        if (!TryComp<ItemComponent>(entity, out var clothingItem) || !TryComp<ItemComponent>(moduleUid, out var webbingItem))
            return false;

        if (_container.TryGetContainingContainer(entity.Owner, out var containing))
        {
            if (TryComp<StorageComponent>(containing.Owner, out var storage) && storage.StoredItems.ContainsKey(entity))
            {
                handled = true;
                UserPopup(Loc.GetString("mc-module-cannot-in-storage"));
                return false;
            }
        }

        var container = _container.EnsureContainer<ContainerSlot>(entity, entity.Comp.Container);
        if (container.Count > 0 || !_container.Insert(moduleUid, container))
            return false;

        EntityManager.AddComponents(entity, module.Components);

        if (userUid is not null)
            _speedModifier.RefreshMovementSpeedModifiers(userUid.Value);

        entity.Comp.UnequippedSize = clothingItem.Size;
        _item.SetSize(entity, webbingItem.Size);

        handled = true;
        return true;

        void UserPopup(string message)
        {
            if (userUid is not null)
                _popup.PopupClient(message, userUid, PopupType.LargeCaution);
        }
    }

    private void Detach(Entity<MCArmorModularClothingComponent> entity, EntityUid user)
    {
        if (TerminatingOrDeleted(entity) || !entity.Comp.Running)
            return;

        if (!TryGetModule((entity, entity), out var module))
            return;

        _container.TryRemoveFromContainer(module.Owner);
        _hands.TryPickupAnyHand(user, module);
        _speedModifier.RefreshMovementSpeedModifiers(user);

        EntityManager.AddComponents(module, module.Comp.Components);

        if (entity.Comp.UnequippedSize is not { } size)
            return;

        entity.Comp.UnequippedSize = null;
        _item.SetSize(entity, size);
    }

    public bool TryGetModule(Entity<MCArmorModularClothingComponent?> entity, out Entity<MCArmorModuleComponent> module)
    {
        module = default;

        if (!Resolve(entity, ref entity.Comp, false))
            return false;

        if (!_container.TryGetContainer(entity, entity.Comp.Container, out var container) || container.Count <= 0)
            return false;

        var ent = container.ContainedEntities[0];
        if (!TryComp<MCArmorModuleComponent>(ent, out var moduleComponent))
            return false;

        module = (ent, moduleComponent);
        return true;
    }

    private static void OnModuleGetRelayed(Entity<MCArmorComponent> entity, ref MCArmorModuleRelayedEvent<MCArmorGetEvent> args)
    {
        args.Args.ArmorDefinition += entity.Comp.Soft;
    }

    private void OnModuleMovementSpeedModifier(Entity<ClothingSpeedModifierComponent> entity, ref MCArmorModuleRelayedEvent<RefreshMovementSpeedModifiersEvent> args)
    {
        args.Args.ModifySpeed(entity.Comp.WalkModifier, entity.Comp.SprintModifier);
    }

    public void RelayEvent<T>(Entity<MCArmorModularClothingComponent> entity, ref InventoryRelayedEvent<T> args)
    {
        var ev = new MCArmorModuleRelayedEvent<T>(args.Args);
        if (entity.Comp.ModuleUid is not {} moduleUid)
            return;

        RaiseLocalEvent(moduleUid, ev);
        args.Args = ev.Args;
    }
}
