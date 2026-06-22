using Content.Shared._MC.Armor.Modules.Core.Components;

namespace Content.Shared._MC.Armor.Modules.Core.Events;

[ByRefEvent]
public readonly record struct MCArmorModuleAttachedEvent(Entity<MCArmorModularClothingComponent> Armor, Entity<MCArmorModuleComponent> Module, EntityUid? User);
