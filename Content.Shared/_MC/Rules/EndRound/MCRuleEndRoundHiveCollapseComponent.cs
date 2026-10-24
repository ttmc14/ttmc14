using Robust.Shared.GameStates;

namespace Content.Shared._MC.Rules.EndRound;

[RegisterComponent, NetworkedComponent]
public sealed partial class MCRuleEndRoundHiveCollapseComponent : Component
{
    [DataField]
    public string EndRuleId = "MajorMarineVictory";
}
