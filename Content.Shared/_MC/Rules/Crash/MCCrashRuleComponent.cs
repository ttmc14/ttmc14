using Content.Shared._MC.Rules.Base;
using Content.Shared._RMC14.Weapons.Ranged.IFF;
using Content.Shared.Roles;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Utility;

namespace Content.Shared._MC.Rules.Crash;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MCCrashRuleComponent : Component, IRulePlanet, IRuleRecalculatePower
{
    [DataField, AutoNetworkedField]
    public EntProtoId<IFFFactionComponent> MarineFaction = "FactionMarine";

    [DataField, AutoNetworkedField]
    public ProtoId<JobPrototype> QueenJob = "MCXenoQueen";

    [DataField, AutoNetworkedField]
    public ProtoId<JobPrototype> ShrikeJob = "MCXenoShrike";

    [DataField, AutoNetworkedField]
    public EntProtoId QueenEnt = "MCXenoQueen";

    [DataField, AutoNetworkedField]
    public EntProtoId ShrikeEnt = "MCXenoShrike";

    [DataField, AutoNetworkedField]
    public ProtoId<JobPrototype> XenoSelectableJob = "MCXenoSelectableXeno";

    [DataField, AutoNetworkedField]
    public EntProtoId LarvaEnt = "MCXenoLarva";

    [DataField, AutoNetworkedField]
    public ResPath Thunderdome = new("/Maps/_RMC14/thunderdome.yml");

    [DataField, AutoNetworkedField]
    public EntityUid? XenoMap { get; set; }

    // Global

#if !FULL_RELEASE
    [DataField, AutoNetworkedField]
    public TimeSpan ShuttleCrushTime = TimeSpan.FromSeconds(15);
#else
    [DataField, AutoNetworkedField]
    public TimeSpan ShuttleCrushTime = TimeSpan.FromMinutes(10);
#endif

    // Marine

    [DataField, AutoNetworkedField]
    public TimeSpan MarineRespawnTime = TimeSpan.FromMinutes(15);

    // Xenos

    [DataField, AutoNetworkedField]
    public List<EntProtoId> XenoRestrictedCastes = new();

    [DataField, AutoNetworkedField]
    public TimeSpan XenoRespawnTime = TimeSpan.FromMinutes(3);

    [DataField, AutoNetworkedField]
    public TimeSpan XenoSwapTimer = TimeSpan.FromMinutes(5);

    [DataField]
    public MCCrashRuleResult Result = MCCrashRuleResult.None;

    [AutoNetworkedField]
    public bool PowerRecalculated { get; set; }

    // TODO: starting_squad
    // TODO: evo_requirements
    // /datum/xeno_caste/king = 14
    // /datum/xeno_caste/queen = 10
    // /datum/xeno_caste/hivelord = 5
}

public interface IRuleRecalculatePower
{
    bool PowerRecalculated { get; set; }
}

[Serializable, NetSerializable]
public enum MCCrashRuleResult
{
    None,
    MajorMarineVictory,
    MinorMarineVictory,
    MajorXenoVictory,
    MinorXenoVictory,
}
