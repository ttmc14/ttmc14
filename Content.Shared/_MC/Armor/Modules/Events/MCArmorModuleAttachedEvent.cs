using Content.Shared._MC.Armor.Modules.Components;

namespace Content.Shared._MC.Armor.Modules.Events;

[ByRefEvent]
public readonly record struct MCArmorModuleAttachedEvent(Entity<MCArmorModularClothingComponent> Armor, Entity<MCArmorModuleComponent> Module, EntityUid? User);
