using Content.Shared._RMC14.Xenonids.Pheromones;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared._MC.Xeno.Structures.PheromoneTowers.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(fieldDeltas: true)]
public sealed partial class MCXenoStructurePheromoneTowerComponent : Component
{
    [DataField, AutoNetworkedField]
    public XenoPheromones SelectedType = XenoPheromones.Frenzy;
}

[Serializable, NetSerializable]
public enum MCXenoStructurePheromoneTowerLayers
{
    Layer,
}
