namespace Content.Shared._MC.Armor.Modules.Core.Events;

[ByRefEvent]
public readonly record struct MCArmorModuleDetachedEvent(EntityUid Armor, EntityUid Module, EntityUid? User);
