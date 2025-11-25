using Content.Shared.Inventory;

namespace Content.Shared._MC.Armor;

[ByRefEvent]
public struct MCArmorGetEvent : IInventoryRelayEvent
{
    public SlotFlags TargetSlots { get; }
    public MCArmorDefinition ArmorDefinition;

    public MCArmorGetEvent(SlotFlags targetSlots, MCArmorDefinition armorDefinition)
    {
        TargetSlots = targetSlots;
        ArmorDefinition = armorDefinition;
    }
}
