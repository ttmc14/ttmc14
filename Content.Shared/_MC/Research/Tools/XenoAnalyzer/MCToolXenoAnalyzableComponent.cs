using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._MC.Research.Tools.XenoAnalyzer;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(fieldDeltas: true)]
public sealed partial class MCToolXenoAnalyzableComponent : Component
{
    [DataField, AutoNetworkedField]
    public EntProtoId RewardProtId = "MCResearchResourceTier1";

    [DataField, AutoNetworkedField]
    public bool Researched;
}
