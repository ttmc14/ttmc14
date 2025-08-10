using Content.Shared._MC.Xeno.Hive.Prototypes;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Shared._MC.Xeno.Blessings.Prototypes;

[Prototype("MCXenoBlessingsEntry")]
public sealed class MCXenoBlessingsEntryPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; } = string.Empty;

    [DataField]
    public SpriteSpecifier.Rsi Icon = new(new ResPath("/Textures/_MC/Structures/Xeno/silo.rsi"), "icon");

    [DataField]
    public LocId Name;

    [DataField]
    public LocId Description;

    [DataField]
    public ProtoId<MCXenoBlessingsGroupPrototype> Group;

    [DataField]
    public ProtoId<MCXenoHivePsypointTypePrototype> CostType;

    [DataField]
    public int Cost;

    [DataField]
    public EntProtoId? Entity;

    [DataField]
    public TimeSpan Time;
}
