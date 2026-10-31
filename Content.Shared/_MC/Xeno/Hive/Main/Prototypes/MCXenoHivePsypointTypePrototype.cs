using Robust.Shared.Prototypes;

namespace Content.Shared._MC.Xeno.Hive.Prototypes;

[Prototype("MCXenoHivePsypointType")]
public sealed class MCXenoHivePsypointTypePrototype : IPrototype
{
    [IdDataField]
    public string ID { get; } = string.Empty;

    [DataField]
    public LocId Name;
}
