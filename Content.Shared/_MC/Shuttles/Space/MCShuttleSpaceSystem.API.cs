using Content.Shared._MC.Shuttles.Space.Components;
using JetBrains.Annotations;
using Robust.Shared.Map;

namespace Content.Shared._MC.Shuttles.Space;

public sealed partial class MCShuttleSpaceSystem
{
    [PublicAPI]
    public void EnsureMap(string id, out MapId mapId, out Entity<MCShuttleSpaceComponent> entity)
    {
        if (Get(id, out mapId, out entity))
            return;

        mapId = Create(id, out entity);
    }
}
