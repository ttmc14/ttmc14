using Content.Shared.Roles;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._MC.Rules.Common;

[RegisterComponent]
public sealed partial class MCRuleLarvaReinforcementsComponent : Component, ISerializationHooks
{
    [DataField(required: true)]
    public int PointsPerLarva;

    [DataField(required: true)]
    public int MinimumPopulation;

    [DataField]
    public Dictionary<ProtoId<JobPrototype>, float> LarvaPointsByJob = [];

    void ISerializationHooks.AfterDeserialization()
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(PointsPerLarva);
        ArgumentOutOfRangeException.ThrowIfNegative(MinimumPopulation);
    }
}
