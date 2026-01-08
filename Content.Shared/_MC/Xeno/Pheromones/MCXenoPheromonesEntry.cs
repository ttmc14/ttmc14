using Content.Shared._RMC14.Xenonids.Pheromones;
using Robust.Shared.Serialization;

namespace Content.Shared._MC.Xeno.Pheromones;

[DataDefinition, Serializable, NetSerializable]
public partial struct MCXenoPheromonesEntry
{
    [DataField]
    public XenoPheromones Pheromones;

    [DataField]
    public int Range;

    [DataField]
    public float Multiplier;

    public MCXenoPheromonesEntry(XenoPheromones pheromones, int range, float multiplier)
    {
        Pheromones = pheromones;
        Range = range;
        Multiplier = multiplier;
    }
}
