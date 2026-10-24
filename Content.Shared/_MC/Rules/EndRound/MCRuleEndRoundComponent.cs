using Robust.Shared.GameStates;

namespace Content.Shared._MC.Rules.EndRound;

[RegisterComponent, NetworkedComponent]
public sealed partial class MCRuleEndRoundComponent : Component
{
    [ViewVariables]
    public string Result;

    [DataField]
    public string ResultTextPrefix = string.Empty;
}
