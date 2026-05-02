using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._MC.Research.Tools.Excavator;

[RegisterComponent, NetworkedComponent]
public sealed partial class MCExcavationSiteComponent : Component
{
    [DataField]
    public int RewardsMin = 2;

    [DataField]
    public int RewardsMax = 4;

    [DataField]
    public List<EntProtoId> Rewards = new();
}
