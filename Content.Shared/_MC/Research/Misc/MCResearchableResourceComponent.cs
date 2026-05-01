using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._MC.Research.Misc;

[RegisterComponent, NetworkedComponent]
public sealed partial class MCResearchableResourceComponent : Component
{
    [DataField(required: true)]
    public string Category = string.Empty;

    [DataField]
    public Dictionary<string, float> TierProbabilities = new();

    [DataField]
    public List<ResearchRewardCategory> Rewards = new();
}

[DataDefinition, Serializable]
public sealed partial class ResearchRewardCategory
{
    [DataField(required: true)]
    public string Id = string.Empty;

    [DataField]
    public List<ResearchRewardTier> Tiers = new();
}

[DataDefinition, Serializable]
public sealed partial class ResearchRewardTier
{
    [DataField(required: true)]
    public string Id = string.Empty;

    [DataField]
    public List<EntProtoId> Rewards = new();
}
