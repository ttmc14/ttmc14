using Content.Shared._MC.Rules.Base;
using Content.Shared._RMC14.Weapons.Ranged.IFF;
using Content.Shared.Roles;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Utility;

namespace Content.Shared._MC.Rules;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MCDistressSignalRuleComponent : Component, IRulePlanet
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

    // Marine

    [DataField, AutoNetworkedField]
    public TimeSpan MarineRespawnTime = TimeSpan.FromMinutes(15);

    [DataField]
    public MCDisstressRuleResult Result = MCDisstressRuleResult.None;
}

[Serializable, NetSerializable]
public enum MCDisstressRuleResult
{
    None,
    MajorMarineVictory,
    MinorMarineVictory,
    MajorXenoVictory,
    MinorXenoVictory,
}
