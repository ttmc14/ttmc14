using Content.Shared._RMC14.Xenonids.Pheromones;
using Content.Shared.FixedPoint;
using Robust.Shared.Containers;
using Robust.Shared.Prototypes;

namespace Content.Shared._MC.Xeno.Pheromones.Relay;

public sealed class MCXenoPheromonesRelaySystem : EntitySystem
{
    private const string RelayContainerId = "mc_pheromone_relays";
    private static readonly EntProtoId RelayProtoId = "MCXenoPheromoneRelay";

    [Dependency] private readonly SharedContainerSystem _container = null!;

    public void AddRelayPheromones(EntityUid uid, MCXenoPheromonesEntry entry)
    {
        AddRelayPheromones(uid, entry.Pheromones, entry.Range, entry.Multiplier);
    }

    public void AddRelayPheromones(EntityUid uid, XenoPheromones pheromones, int range, float multiplier)
    {
        _container.EnsureContainer<Container>(uid, RelayContainerId);
        if (!TrySpawnInContainer(RelayProtoId, uid, RelayContainerId, out var relayUid))
            return;

        var component = EnsureComp<XenoPheromonesComponent>(relayUid.Value);
        component.PheromonesPlasmaCost = 0;
        component.PheromonesPlasmaUpkeep = 0;
        component.PheromonesRange = range;
        component.PheromonesMultiplier = FixedPoint2.New(multiplier);
        Dirty(relayUid.Value, component);

        var componentActive = EnsureComp<XenoActivePheromonesComponent>(relayUid.Value);
        componentActive.Pheromones = pheromones;
        Dirty(relayUid.Value, componentActive);
    }
}
