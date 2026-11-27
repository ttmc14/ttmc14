using Robust.Shared.Prototypes;

namespace Content.Shared._MC.ASRS;

[Prototype("MCASRSAccount")]
public sealed class MCASRSAccountPrototype : IPrototype
{
    public static readonly ProtoId<MCASRSAccountPrototype> Default = "Default";

    [IdDataField]
    public string ID { get; private set; } = string.Empty;
}
