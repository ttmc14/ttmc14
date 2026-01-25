using Content.Shared.Inventory;

namespace Content.Shared._MC.Armor.Events;

[ByRefEvent]
public struct MCArmorGetEvent : IInventoryRelayEvent
{
    public SlotFlags TargetSlots { get; }

    public MCArmorDefinition SoftArmor;
    public MCArmorDefinition HardArmor;

    public MCArmorGetEvent(SlotFlags targetSlots)
    {
        TargetSlots = targetSlots;
    }
}
