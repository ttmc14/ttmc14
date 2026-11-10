using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._MC.Xeno.QueueJoin.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
[Access(typeof(MCXenoHiveQueueJoinSystem))]
public sealed partial class MCXenoHiveQueueJoinComponent : Component
{
    [DataField]
    public EntProtoId ActionId = "MCJoinXenoAction";

    [ViewVariables, AutoNetworkedField]
    public EntityUid? Action;
}
