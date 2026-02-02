namespace Content.Shared._MC.Armor.Modules.Events;

[ByRefEvent]
public readonly record struct MCArmorModuleDetachedEvent(EntityUid Armor, EntityUid Module, EntityUid? User);
