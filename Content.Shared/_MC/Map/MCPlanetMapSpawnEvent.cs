using Robust.Shared.Map.Components;
using Robust.Shared.Prototypes;

namespace Content.Shared._MC.Map;

[ByRefEvent]
public record struct MCPlanetMapSpawnEvent(Entity<MapComponent> Entity, EntityPrototype Prototype);
