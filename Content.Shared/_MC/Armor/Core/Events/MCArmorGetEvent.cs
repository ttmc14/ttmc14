using Content.Shared._MC.Armor.Core.Components;
using Content.Shared.Inventory;

namespace Content.Shared._MC.Armor.Core.Events;

[ByRefEvent]
public struct MCArmorGetEvent(SlotFlags targetSlots) : IInventoryRelayEvent
{
    public SlotFlags TargetSlots { get; } = targetSlots;

    public MCArmorDefinition SoftArmor;
    public MCArmorDefinition HardArmor;
}
