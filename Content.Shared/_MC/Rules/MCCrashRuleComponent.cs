using Content.Shared._RMC14.Weapons.Ranged.IFF;
using Content.Shared.Roles;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Shared._MC.Rules;

[RegisterComponent, NetworkedComponent]
public sealed partial class MCCrashRuleComponent : Component
{
    [DataField]
    public EntProtoId<IFFFactionComponent> MarineFaction = "FactionMarine";

    [DataField]
    public ProtoId<JobPrototype> QueenJob = "MCXenoQueen";

    [DataField]
    public ProtoId<JobPrototype> ShrikeJob = "MCXenoShrike";

    [DataField]
    public EntProtoId QueenEnt = "MCXenoQueen";

    [DataField]
    public EntProtoId ShrikeEnt = "MCXenoShrike";

    [DataField]
    public ProtoId<JobPrototype> XenoSelectableJob = "MCXenoSelectableXeno";

    [DataField]
    public EntProtoId LarvaEnt = "MCXenoLarva";

    [DataField]
    public ResPath Thunderdome = new("/Maps/_RMC14/thunderdome.yml");

    [DataField]
    public EntityUid? XenoMap;

    // Global

#if !FULL_RELEASE
    [DataField]
    public TimeSpan ShuttleCrushTime = TimeSpan.FromSeconds(15);
#else
    [DataField]
    public TimeSpan ShuttleCrushTime = TimeSpan.FromMinutes(10);
#endif

    // Marine

    [DataField]
    public TimeSpan MarineRespawnTime = TimeSpan.FromMinutes(15);

    // Xenos

    [DataField]
    public List<EntProtoId> XenoRestrictedCastes = new();

    [DataField]
    public TimeSpan XenoRespawnTime = TimeSpan.FromMinutes(3);

    [DataField]
    public TimeSpan XenoSwapTimer = TimeSpan.FromMinutes(5);

    // TODO: starting_squad

    // TODO: evo_requirements
    // /datum/xeno_caste/king = 14
    // /datum/xeno_caste/queen = 10
    // /datum/xeno_caste/hivelord = 5
}
