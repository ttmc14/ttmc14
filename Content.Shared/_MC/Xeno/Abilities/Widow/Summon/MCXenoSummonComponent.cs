using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._MC.Xeno.Abilities.Widow.Summon;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MCXenoSummonComponent : Component
{
    [DataField]
    public EntProtoId ProtoId = "MCXenoSummonSpiderling";

    [DataField]
    public int Limit = 5;

    [ViewVariables, AutoNetworkedField]
    public List<EntityUid> SummonUids = new();
}
