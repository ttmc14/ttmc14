using Content.Shared._MC.Engineering.Miners.Components;

namespace Content.Shared._MC.Engineering.Miners.Events.Equipment;

[ByRefEvent]
public readonly record struct MCMinerModuleAttachedEvent(Entity<MCMinerModuleContainerComponent> Entity, EntityUid Module);
