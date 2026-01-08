using Robust.Shared.GameStates;

namespace Content.Shared._MC.Xeno.Pheromones;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MCXenoPheromonesPassiveComponent : Component
{
    [DataField, AutoNetworkedField]
    public List<MCXenoPheromonesEntry> Entries = new();
}
