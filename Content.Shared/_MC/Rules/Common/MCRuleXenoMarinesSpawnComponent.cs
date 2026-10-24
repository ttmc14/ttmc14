using Content.Shared._RMC14.Weapons.Ranged.IFF;
using Content.Shared.Roles;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._MC.Rules.Common;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MCRuleXenoMarinesSpawnComponent : Component
{
    [ViewVariables, AutoNetworkedField]
    public bool Spawned;

    #region Xeno

    [DataField]
    public List<MCJobRolePair> XenoUnique = new()
    {
        new MCJobRolePair("MCXenoQueen"),
        new MCJobRolePair("MCXenoShrike"),
    };

    [DataField]
    public MCJobRolePair XenoOverflow = new("MCXenoSelectableXeno", "MCXenoRafik");

    #endregion
}

[DataDefinition]
public partial struct MCJobRolePair
{
    [DataField]
    public ProtoId<JobPrototype> JobId;

    [DataField]
    public EntProtoId EntityId;

    public MCJobRolePair(string id)
    {
        JobId = id;
        EntityId = id;
    }

    public MCJobRolePair(ProtoId<JobPrototype> jobId, EntProtoId entityId)
    {
        JobId = jobId;
        EntityId = entityId;
    }
}
