using Robust.Shared.Serialization;

namespace Content.Shared._MC.Rules.Common;

[RegisterComponent]
public sealed partial class MCRuleReserveGrowthComponent : Component, ISerializationHooks
{
    [DataField(required: true)]
    public int MarinesPerXeno;

    [DataField(required: true)]
    public int LivesPerPlayer;

    [DataField(required: true)]
    public int PopulationThreshold;

    void ISerializationHooks.AfterDeserialization()
    {
        ArgumentOutOfRangeException.ThrowIfNegative(MarinesPerXeno);
        ArgumentOutOfRangeException.ThrowIfNegative(LivesPerPlayer);
        ArgumentOutOfRangeException.ThrowIfNegative(PopulationThreshold);
    }
}
