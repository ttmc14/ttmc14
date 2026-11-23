using Robust.Shared.Prototypes;

namespace Content.Shared._MC.ASRS;

[Prototype("MCASRSAccount")]
public sealed class MCASRSAccountPrototype : IPrototype
{
    public static readonly ProtoId<MCASRSAccountPrototype> Default = "Default";

    [IdDataField]
    public string ID { get; private set; } = string.Empty;

    /// <summary>
    /// Initial supply points for this account. Defaults to the mc.asrs.starting_balance setting.
    /// </summary>
    [DataField]
    public int? StartingBalance;
}
