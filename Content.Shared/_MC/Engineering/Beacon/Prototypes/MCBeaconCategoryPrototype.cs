using Robust.Shared.Prototypes;

namespace Content.Shared._MC.Engineering.Beacon.Prototypes;

[Prototype("MCBeaconCategory")]
public sealed class MCBeaconCategoryPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; } = string.Empty;
}
