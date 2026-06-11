using Content.Shared._MC.Xeno.Structures.PheromoneTowers;
using Robust.Shared.GameStates;

namespace Content.Shared._RMC14.Xenonids.Pheromones;

// Component for adding pheromones to objects, e.g. spore resin fruit
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
[Access(typeof(SharedXenoPheromonesSystem), typeof(MCXenoStructurePheromoneTowerSystem))] // mc-changes
public sealed partial class XenoPheromonesObjectComponent : Component
{
    [DataField, AutoNetworkedField]
    public XenoPheromones Pheromones = XenoPheromones.Recovery;
}
