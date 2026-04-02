using Content.Shared.Inventory;

namespace Content.Shared._MC.ZLevels.Events;

[ByRefEvent]
public struct MCZLevelFallStunModifierEvent() : IInventoryRelayEvent
{
    public SlotFlags TargetSlots { get; init; }
    public float Modifier = 1f;
}
