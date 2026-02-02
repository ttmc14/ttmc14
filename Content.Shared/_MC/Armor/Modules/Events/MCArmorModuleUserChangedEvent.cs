namespace Content.Shared._MC.Armor.Modules.Events;

[ByRefEvent]
public readonly record struct MCArmorModuleUserChangedEvent(EntityUid? OldUser, EntityUid? NewUser);
