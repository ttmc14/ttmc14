using Content.Shared.Roles;
using Robust.Shared.Prototypes;

namespace Content.Shared._MC.Rules.Warfare;

/// <summary>
/// Tracks reinforcements that have not yet been issued in this round.
/// </summary>
[RegisterComponent]
public sealed partial class MCRuleWarfareComponent : Component
{
    [DataField]
    public int MarineReserve = 60;

    [DataField]
    public int XenoReserve = 20;

    [DataField]
    public int PopulationThreshold = 20;

    [DataField]
    public Dictionary<ProtoId<JobPrototype>, float> LarvaPointsByJob = [];

    [ViewVariables]
    public TimeSpan NextBalance;

    [ViewVariables]
    public bool Landed;
}
