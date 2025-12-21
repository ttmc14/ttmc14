using Robust.Shared.GameStates;

namespace Content.Shared._MC.Xeno.Constructions.Spawner;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MCXenoStructureSpawnerTargetComponent : Component
{
    [ViewVariables, AutoNetworkedField]
    public EntityUid Origin;
}
