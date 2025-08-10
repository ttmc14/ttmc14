using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._MC.Xeno.Hive.Systems;

public abstract class MCSharedXenoHiveSystem : MCEntitySystemSingleton<MCXenoHiveSingletonComponent>
{
    [ViewVariables]
    public EntityUid? DefaultHive => Inst.Comp.DefaultHive;
}

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MCXenoHiveSingletonComponent : Component
{
    [DataField, AutoNetworkedField]
    public string DefaultHiveName = "xeno hive";

    [DataField, AutoNetworkedField]
    public EntProtoId DefaultHiveId = "MCXenoHive";

    [DataField, AutoNetworkedField]
    public EntityUid? DefaultHive;
}
