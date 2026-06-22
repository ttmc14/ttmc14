using Content.Shared._MC.Armor.Core.Components;
using Content.Shared._MC.Armor.Core.Events;
using Content.Shared._MC.Armor.Modules.Core.Components;
using Content.Shared._MC.Armor.Modules.Core.Events;
using Content.Shared._MC.Weapon.Vali.Events;
using Content.Shared._RMC14.Atmos;
using Content.Shared.Actions;
using Content.Shared.Clothing;
using Content.Shared.Damage;
using Content.Shared.Examine;
using Content.Shared.Inventory;
using Content.Shared.Movement.Systems;
using Content.Shared.Verbs;
using Content.Shared.Weapons.Melee.Events;

namespace Content.Shared._MC.Armor.Modules.Core;

public sealed class MCArmorModuleRelaySystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<InventoryComponent, ExaminedEvent>(InventoryRelayEvent);
        SubscribeLocalEvent<InventoryComponent, MCWeaponValiMeleeHitEvent>(InventoryRelayEvent);
        SubscribeLocalEvent<InventoryComponent, RMCIgniteAttemptEvent>(InventoryRelayEvent);
        SubscribeLocalEvent<InventoryComponent, RMCGetFireImmunityEvent>(InventoryRelayEvent);

        // Other relay shit
        SubscribeLocalEvent<MCArmorModularClothingComponent, InventoryRelayedEvent<RefreshMovementSpeedModifiersEvent>>(RelayEvent);
        SubscribeLocalEvent<MCArmorModularClothingComponent, InventoryRelayedEvent<MCArmorGetEvent>>(RelayEvent);
        SubscribeLocalEvent<MCArmorModularClothingComponent, InventoryRelayedEvent<GetVerbsEvent<ExamineVerb>>>(RelayEvent);
        SubscribeLocalEvent<MCArmorModularClothingComponent, InventoryRelayedEvent<DamageModifyEvent>>(RelayEvent);
        SubscribeLocalEvent<MCArmorModularClothingComponent, InventoryRelayedEvent<ExaminedEvent>>(RelayEvent);
        SubscribeLocalEvent<MCArmorModularClothingComponent, InventoryRelayedEvent<AttackedEvent>>(RelayEvent);
        SubscribeLocalEvent<MCArmorModularClothingComponent, InventoryRelayedEvent<MCWeaponValiMeleeHitEvent>>(RelayEvent);
        SubscribeLocalEvent<MCArmorModularClothingComponent, InventoryRelayedEvent<RMCIgniteAttemptEvent>>(RelayEvent);
        SubscribeLocalEvent<MCArmorModularClothingComponent, InventoryRelayedEvent<RMCGetFireImmunityEvent>>(RelayEvent);

        SubscribeLocalEvent<MCArmorModularClothingComponent, GetItemActionsEvent>(RelayEvent);
        SubscribeLocalEvent<MCArmorComponent, MCArmorModuleRelayedEvent<MCArmorGetEvent>>(OnModuleGetRelayed);
        SubscribeLocalEvent<ClothingSpeedModifierComponent, MCArmorModuleRelayedEvent<RefreshMovementSpeedModifiersEvent>>(OnModuleMovementSpeedModifier);
    }

    private static void OnModuleGetRelayed(Entity<MCArmorComponent> entity, ref MCArmorModuleRelayedEvent<MCArmorGetEvent> args)
    {
        args.Args.SoftArmor += entity.Comp.Soft;
        args.Args.HardArmor += entity.Comp.Hard;
    }

    private static void OnModuleMovementSpeedModifier(Entity<ClothingSpeedModifierComponent> entity, ref MCArmorModuleRelayedEvent<RefreshMovementSpeedModifiersEvent> args)
    {
        args.Args.ModifySpeed(entity.Comp.WalkModifier, entity.Comp.SprintModifier);
    }

    public void InventoryRelayEvent<T>(Entity<InventoryComponent> inventory, ref T args)
    {
        var ev = new InventoryRelayedEvent<T>(args);
        var enumerator = new InventorySystem.InventorySlotEnumerator(inventory);
        while (enumerator.NextItem(out var item))
        {
            RaiseLocalEvent(item, ev);
        }

        args = ev.Args;
    }

    public void RelayEvent<T>(Entity<MCArmorModularClothingComponent> entity, ref T args)
    {
        var ev = new MCArmorModuleRelayedEvent<T>(args);
        foreach (var slot in entity.Comp.Slots)
        {
            if (slot.Module is null)
                continue;

            RaiseLocalEvent(slot.Module.Value, ev);
        }

        args = ev.Args;
    }

    public void RelayEvent<T>(Entity<MCArmorModularClothingComponent> entity, ref InventoryRelayedEvent<T> args)
    {
        var ev = new MCArmorModuleRelayedEvent<T>(args.Args);
        foreach (var slot in entity.Comp.Slots)
        {
            if (slot.Module is null)
                continue;

            RaiseLocalEvent(slot.Module.Value, ev);
        }

        args.Args = ev.Args;
    }
}
