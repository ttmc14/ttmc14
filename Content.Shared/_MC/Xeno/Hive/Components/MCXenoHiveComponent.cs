using Content.Shared._MC.Xeno.Hive.Prototypes;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._MC.Xeno.Hive.Components;

// TODO: This is a theoretical component that is intended to replace RMC systems in the future.
// Don’t expect it to actually exist in the game - technically.
// It does not do anything.
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MCXenoHiveComponent : Component
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
    public bool CanEvolveWithoutLeader;

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
    public int BurrowedLarva;

    [DataField, AutoNetworkedField]
    public int BurrowedLarvaSlotFactor = 4;
}
