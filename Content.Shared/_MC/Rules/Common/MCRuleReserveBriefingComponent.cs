namespace Content.Shared._MC.Rules.Common;

[RegisterComponent]
public sealed partial class MCRuleReserveBriefingComponent : Component
{
    [DataField(required: true)]
    public LocId MarineBriefing;

    [DataField(required: true)]
    public LocId XenoBriefing;
}
