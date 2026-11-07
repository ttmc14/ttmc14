using Robust.Shared.Prototypes;
using Robust.Shared.GameStates;

namespace Content.Shared._MC.Spawners;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MCPVEXenoSpawnPointComponent : Component
{
    [DataField, AutoNetworkedField]
    public EntProtoId SpawnPrototype = "CMXenoDrone";

    [DataField, AutoNetworkedField]
    public TimeSpan SpawnInterval = TimeSpan.FromSeconds(30);

    [DataField, AutoNetworkedField]
    public EntProtoId EnemyFaction = "FactionXeno";

    [DataField, AutoNetworkedField]
    public bool SpawnSmokeOnSpawn = true;

    [DataField, AutoNetworkedField]
    public EntProtoId SmokePrototype = "Foam";

    [DataField, AutoNetworkedField]
    public string SmokeReagent = "MCSmokeXenoAcidBoiler";

    [DataField, AutoNetworkedField]
    public float SmokeReagentAmount = 10f;

    [DataField, AutoNetworkedField]
    public float SmokeDuration = 10f;

    [DataField, AutoNetworkedField]
    public int SmokeSpread = 3;

    [DataField, AutoNetworkedField]
    public int SpawnedCount = 0;
}
