using Robust.Shared.Prototypes;

namespace Content.Shared._MC.Damage.Integrity;

[Prototype("MCIntegrity")]
public sealed class MCIntegrityPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; } = string.Empty;
}
