using Robust.Shared.Prototypes;

namespace Content.Shared._MC.Xeno.Blessings.Prototypes;

[Prototype("MCXenoBlessingsGroup")]
public sealed class MCXenoBlessingsGroupPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; } = string.Empty;

    [DataField]
    public LocId Title;
}
