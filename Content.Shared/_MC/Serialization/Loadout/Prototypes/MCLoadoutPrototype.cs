using Content.Shared._MC.Serialization.Loadout.Data;
using Robust.Shared.Prototypes;

namespace Content.Shared._MC.Serialization.Loadout.Prototypes;

[Prototype]
public sealed class MCLoadoutPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; } = string.Empty;

    [DataField]
    public MCLoadout Loadout = new();
}
