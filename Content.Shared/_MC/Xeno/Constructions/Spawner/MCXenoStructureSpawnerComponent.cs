using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._MC.Xeno.Constructions.Spawner;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MCXenoStructureSpawnerComponent : Component
{
    [DataField, AutoNetworkedField]
    public TimeSpan NextSpawn;

    [DataField, AutoNetworkedField]
    public List<EntProtoId> Entry = new()
    {
        "MCXenoMinionBaneling",
        "MCXenoMinionBeetle",
        "MCXenoMinionMantis",
        "MCXenoMinionNymph",
        "MCXenoMinionScorpion",
    };

    [DataField, AutoNetworkedField]
    public List<EntityUid> Entities = new();

    [DataField, AutoNetworkedField]
    public int MinMobs = 1;

    [DataField, AutoNetworkedField]
    public float MobsPerPlayer = 0.15f;

    [DataField, AutoNetworkedField]
    public float RespawnMultiplier = 1;

    [DataField, AutoNetworkedField]
    public TimeSpan RespawnPerPlayer = TimeSpan.FromSeconds(3.6);

    [DataField, AutoNetworkedField]
    public TimeSpan MinRespawn = TimeSpan.FromSeconds(45);
}
