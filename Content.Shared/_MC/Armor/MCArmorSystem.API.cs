using Content.Shared._MC.Armor.Events;
using Content.Shared.Inventory;

namespace Content.Shared._MC.Armor;

public sealed partial class MCArmorSystem
{
    public MCArmorDefinition? GetSoftArmor(Entity<MCArmorComponent?> entity, SlotFlags slotFlags = SlotFlags.OUTERCLOTHING | SlotFlags.INNERCLOTHING)
    {
        if (!TryGetArmor(entity,out var soft, out _, slotFlags: slotFlags))
            return null;

        return soft;
    }

    public MCArmorDefinition? GetHardArmor(Entity<MCArmorComponent?> entity, SlotFlags slotFlags = SlotFlags.OUTERCLOTHING | SlotFlags.INNERCLOTHING)
    {
        if (!TryGetArmor(entity, out _, out var hard, slotFlags: slotFlags))
            return null;

        return hard;
    }

    public bool TryGetArmor(Entity<MCArmorComponent?> entity, out MCArmorDefinition soft, out MCArmorDefinition hard, SlotFlags slotFlags = SlotFlags.OUTERCLOTHING | SlotFlags.INNERCLOTHING)
    {
        soft = default;
        hard = default;

        if (!Resolve(entity, ref entity.Comp))
            return false;

        var ev = new MCArmorGetEvent(slotFlags);
        RaiseLocalEvent(entity, ref ev);

        soft = ev.SoftArmor;
        hard = ev.HardArmor;
        return true;
    }
}
