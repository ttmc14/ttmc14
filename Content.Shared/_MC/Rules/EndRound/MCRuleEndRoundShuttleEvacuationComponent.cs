using Robust.Shared.GameStates;

namespace Content.Shared._MC.Rules.EndRound;

[RegisterComponent, NetworkedComponent]
public sealed partial class MCRuleEndRoundShuttleEvacuationComponent : Component
{
    [DataField]
    public string EndRuleId = "MinorXenoVictory";
}
