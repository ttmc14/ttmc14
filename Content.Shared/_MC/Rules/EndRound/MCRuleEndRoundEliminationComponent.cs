namespace Content.Shared._MC.Rules.EndRound;

[RegisterComponent]
public sealed partial class MCRuleEndRoundEliminationComponent : Component
{
    [DataField(required: true)]
    public TimeSpan Delay;

    [DataField(required: true)]
    public string Draw = string.Empty;

    [DataField(required: true)]
    public string MarineVictory = string.Empty;

    [DataField(required: true)]
    public string XenoVictory = string.Empty;
}
