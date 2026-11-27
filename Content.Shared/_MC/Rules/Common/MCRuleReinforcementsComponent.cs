namespace Content.Shared._MC.Rules.Common;

[RegisterComponent]
public sealed partial class MCRuleReinforcementsComponent : Component
{
    [DataField(required: true)]
    public int MarineReserve;

    [DataField(required: true)]
    public int XenoReserve;

    [DataField(required: true)]
    public LocId NoMarines;
}
