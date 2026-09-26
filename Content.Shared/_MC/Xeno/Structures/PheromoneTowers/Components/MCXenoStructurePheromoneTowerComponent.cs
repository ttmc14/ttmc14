using Content.Shared._RMC14.Xenonids.Pheromones;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared._MC.Xeno.Structures.PheromoneTowers.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(fieldDeltas: true)]
public sealed partial class MCXenoStructurePheromoneTowerComponent : Component
{
    [DataField, AutoNetworkedField]
    public XenoPheromones Selected = XenoPheromones.Frenzy;

    [DataField, AutoNetworkedField]
    public Dictionary<XenoPheromones, Color> TypeColor = new()
    {
        { XenoPheromones.Frenzy, Color.FromHex("#ff3b3b") },
        { XenoPheromones.Warding, Color.FromHex("#64C864") },
        { XenoPheromones.Recovery, Color.FromHex("#6496FA") },
    };
}

[Serializable, NetSerializable]
public enum MCXenoStructurePheromoneTowerLayers
{
    Layer,
}
