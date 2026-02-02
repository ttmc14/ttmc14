using Content.Shared._MC.Armor.Events;
using Content.Shared._MC.Armor.Modules.Components;
using Content.Shared._MC.Armor.Modules.Events;
using Content.Shared.Clothing;
using Content.Shared.Damage;
using Content.Shared.Examine;
using Content.Shared.Inventory;
using Content.Shared.Movement.Systems;
using Content.Shared.Verbs;

namespace Content.Shared._MC.Armor.Modules.Systems;

public sealed class MCArmorModuleRelaySystem : EntitySystem
{
    [Dependency] private readonly InventorySystem _inventory = null!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<InventoryComponent, ExaminedEvent>(_inventory.RelayEvent);

        SubscribeLocalEvent<MCArmorModularClothingComponent, InventoryRelayedEvent<RefreshMovementSpeedModifiersEvent>>(RelayEvent);
        SubscribeLocalEvent<MCArmorModularClothingComponent, InventoryRelayedEvent<MCArmorGetEvent>>(RelayEvent);
        SubscribeLocalEvent<MCArmorModularClothingComponent, InventoryRelayedEvent<GetVerbsEvent<ExamineVerb>>>(RelayEvent);
        SubscribeLocalEvent<MCArmorModularClothingComponent, InventoryRelayedEvent<DamageModifyEvent>>(RelayEvent, after: new[] { typeof(MCArmorSystem) });
        SubscribeLocalEvent<MCArmorModularClothingComponent, InventoryRelayedEvent<ExaminedEvent>>(RelayEvent);

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
