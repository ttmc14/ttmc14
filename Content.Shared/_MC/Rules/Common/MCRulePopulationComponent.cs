namespace Content.Shared._MC.Rules.Common;

[RegisterComponent]
public sealed partial class MCRulePopulationComponent : Component
{
    [DataField(required: true)]
    public TimeSpan Interval;

    [ViewVariables]
    public TimeSpan NextCheck;
}
