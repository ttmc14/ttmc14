using Content.Shared._MC.Armor.Events;
using Content.Shared.Inventory;

namespace Content.Shared._MC.Armor;

public sealed partial class MCArmorSystem
{
    private static void OnGet(Entity<MCArmorComponent> entity, ref MCArmorGetEvent args)
    {
        args.SoftArmor += entity.Comp.Soft;
        args.HardArmor += entity.Comp.Hard;
    }

    private static void OnInventoryGetRelayed(Entity<MCArmorComponent> entity, ref InventoryRelayedEvent<MCArmorGetEvent> args)
    {
        args.Args.SoftArmor += entity.Comp.Soft;
    }
}
