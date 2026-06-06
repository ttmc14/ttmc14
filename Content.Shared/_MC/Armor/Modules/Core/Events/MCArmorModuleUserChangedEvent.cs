namespace Content.Shared._MC.Armor.Modules.Core.Events;

[ByRefEvent]
public readonly record struct MCArmorModuleUserChangedEvent(EntityUid? OldUser, EntityUid? NewUser);
