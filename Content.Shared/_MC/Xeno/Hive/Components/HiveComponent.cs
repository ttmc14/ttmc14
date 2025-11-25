using Content.Shared._MC.Xeno.Hive.Prototypes;
using Robust.Shared.Prototypes;
// ReSharper disable CheckNamespace

namespace Content.Shared._RMC14.Xenonids.Hive;

public sealed partial class HiveComponent
{
    [DataField, AutoNetworkedField]
    public Dictionary<ProtoId<MCXenoHivePsypointTypePrototype>, int> Psypoints = new();

    [DataField, AutoNetworkedField]
    public TimeSpan RespawnTime = TimeSpan.FromMinutes(3);

    [DataField, AutoNetworkedField]
    public TimeSpan CasteSwapTime = TimeSpan.FromMinutes(5);

    [DataField, AutoNetworkedField]
    public bool CanCollapse = true;

    [DataField, AutoNetworkedField]
    public bool CanLarvaPoints = true;

    [DataField, AutoNetworkedField]
    public bool CanEvolveWithoutRuler;

    [DataField, AutoNetworkedField]
    public List<EntityUid> Rulers = new();

    [DataField, AutoNetworkedField]
    public List<EntProtoId> CaseEvolutionBlock = new()
    {
        // "MCXenoHiveMind",
        "MCXenoWraith",
    };

    [DataField, AutoNetworkedField]
    public Dictionary<EntProtoId, int> CasteEvolutionCountRequire = new()
    {
        { "MCXenoHivelord", 5 },
        { "MCXenoQueen", 10 },
        // { "MCXenoKing", 14 },
    };

    [DataField, AutoNetworkedField]
    public int LarvaPoints;

    [DataField, AutoNetworkedField]
    public int LarvaPointsPerBurrowedLarva = 8;

    [DataField, AutoNetworkedField]
    public int BurrowedLarva;

    [DataField, AutoNetworkedField]
    public int BurrowedLarvaSlotFactor = 4;
}
