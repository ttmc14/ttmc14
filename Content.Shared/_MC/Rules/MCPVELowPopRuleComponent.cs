using Content.Shared._MC.Rules.Base;
using Robust.Shared.Prototypes;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;
using Robust.Shared.GameObjects;
using Content.Shared._RMC14.Weapons.Ranged.IFF;
using Content.Shared.Roles;

namespace Content.Shared._MC.Rules.PVEDefendLowPop;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MCPVEDefendLowPopRuleComponent : Component, IRulePlanet, IRuleRecalculatePower
{
    // --- СЕКЦИЯ МОРПЕХОВ (ИГРОКОВ) ---

    [DataField, AutoNetworkedField]
    public EntProtoId<IFFFactionComponent> MarineFaction = "FactionMarine";

    [DataField, AutoNetworkedField]
    public List<ProtoId<JobPrototype>> AllowedMarineJobs = new();

    [DataField, AutoNetworkedField]
    public TimeSpan MarineRespawnTime = TimeSpan.FromMinutes(7);

    // --- СЕКЦИЯ КСЕНОМОРФОВ (ИИ) ---

    [DataField, AutoNetworkedField]
    public EntProtoId<IFFFactionComponent> EnemyFaction = "FactionXeno";

    [DataField, AutoNetworkedField]
    public List<EntProtoId> XenoPrototypes = new();

    [DataField, AutoNetworkedField]
    public TimeSpan SpawnInterval = TimeSpan.FromSeconds(30);

    [DataField, AutoNetworkedField]
    public int MaxAiCount = 20;

    // === СЕКЦИЯ ДЫМА ПРИ СПАВНЕ КСЕНОМОРФОВ ===

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
    public int SmokeSpread = 6;

    // --- УСЛОВИЯ И СОСТОЯНИЕ РАУНДА ---

    [DataField, AutoNetworkedField]
    public TimeSpan DefendTime = TimeSpan.FromMinutes(35);

    [DataField, AutoNetworkedField]
    public MCPVEDefendLowPopRuleState RoundState = MCPVEDefendLowPopRuleState.NotStarted;

    [DataField]
    public MCPVEDefendLowPopRuleResult Result = MCPVEDefendLowPopRuleResult.None;

    [DataField, AutoNetworkedField]
    public EntityUid? XenoMap { get; set; }

    [AutoNetworkedField]
    public bool PowerRecalculated { get; set; }
}

[Serializable, NetSerializable]
public enum MCPVEDefendLowPopRuleState
{
    NotStarted,
    InProgress,
    Finished
}

public interface IRuleRecalculatePower
{
    bool PowerRecalculated { get; set; }
}

[Serializable, NetSerializable]
public enum MCPVEDefendLowPopRuleResult
{
    None,

    MarinesWin,
    MarinesLose
}
