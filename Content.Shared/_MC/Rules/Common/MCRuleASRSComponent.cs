using Content.Shared._MC.ASRS;
using Robust.Shared.Prototypes;

namespace Content.Shared._MC.Rules.Common;

[RegisterComponent]
public sealed partial class MCRuleASRSComponent : Component
{
    /// <summary>
    /// Supply points assigned to each account when the game rule starts.
    /// </summary>
    [DataField]
    public Dictionary<ProtoId<MCASRSAccountPrototype>, int> StartingBalances = [];
}
