using Content.Shared._MC.Armor.Core.Components;
using Content.Shared._MC.Armor.Core.Events;
using Content.Shared.Inventory;

namespace Content.Shared._MC.Armor.Core;

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

        if (!Resolve(entity, ref entity.Comp, logMissing: false))
            return false;

        var ev = new MCArmorGetEvent(slotFlags);
        RaiseLocalEvent(entity, ref ev);

        var evModify = new MCArmorModifyEvent(ev.SoftArmor, ev.HardArmor);
        RaiseLocalEvent(entity, ref evModify);

        soft = evModify.SoftArmor;
        hard = evModify.HardArmor;
        return true;
    }
}
