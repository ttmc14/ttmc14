namespace Content.Shared._MC.Rules.Common;

[RegisterComponent]
public sealed partial class MCRuleRespawnDelayComponent : Component
{
    [DataField(required: true)]
    public TimeSpan Delay;
}
