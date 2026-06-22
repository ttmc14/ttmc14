using Content.Shared._RMC14.Xenonids.Pheromones;
using Robust.Shared.Serialization;

namespace Content.Shared._MC.Xeno.Constructions.PheromoneTowers.UI;

[Serializable, NetSerializable]
public sealed class MCXenoStructurePheromoneTowerSelectUIMessage(XenoPheromones pheromones) : BoundUserInterfaceMessage
{
    public readonly XenoPheromones SelectedType = pheromones;
}
