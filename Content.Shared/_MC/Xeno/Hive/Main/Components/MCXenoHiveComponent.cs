using Content.Shared._MC.Xeno.Hive.Prototypes;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

// ReSharper disable UseCollectionExpression

namespace Content.Shared._MC.Xeno.Hive.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(fieldDeltas: true)]
public sealed partial class MCXenoHiveComponent : Component
{
    [DataField, AutoNetworkedField]
    public List<EntityUid> Rulers = new();

    [DataField, AutoNetworkedField]
    public MCXenoHiveConfiguration Configuration = new();

    [DataField, AutoNetworkedField]
    public Color Color = Color.White;

    #region Psy points

    [DataField, AutoNetworkedField]
    public Dictionary<ProtoId<MCXenoHivePsypointTypePrototype>, int> Psypoints = new();

    #endregion

    #region Larva points

    [DataField, AutoNetworkedField]
    public int LarvaPoints;

    [DataField, AutoNetworkedField]
    public int LarvaPointsPerBurrowedLarva = 8;

    [DataField, AutoNetworkedField]
    public int BurrowedLarva;

    [DataField, AutoNetworkedField]
    public EntProtoId BurrowedLarvaId = "MCXenoRafik";

    [DataField, AutoNetworkedField]
    public bool LateJoinGainLarva = true;

    #endregion

    #region Collapse

    [DataField, AutoNetworkedField]
    public bool Collapsed;

    [DataField]
    public Dictionary<MCXenoHiveCollapseType, TimeSpan> Collapse = new();

    #endregion
}

[DataDefinition, Serializable, NetSerializable]
public sealed partial class MCXenoHiveConfiguration
{
    [DataField]
    public MCXenoHiveConfigGeneral General = new();

    [DataField]
    public MCXenoHiveConfigEvolution Evolution = new();
}

[DataDefinition, Serializable, NetSerializable]
public sealed partial class MCXenoHiveConfigGeneral
{
    [DataField]
    public bool AllowCollapse = true;

    [DataField]
    public bool AllowHarvestLarvaPoints = true;

    [DataField]
    public bool AllowGenerateLarvaPoint = true;

    [DataField]
    public Dictionary<int, int> AdditionalSlots = new();

    [DataField]
    public TimeSpan RespawnTime = TimeSpan.FromMinutes(3);

    [DataField]
    public TimeSpan CasteSwapTime = TimeSpan.FromMinutes(5);

    [DataField]
    public Dictionary<MCXenoHiveCollapseType, TimeSpan> CollapseTime = new()
    {
        { MCXenoHiveCollapseType.Ruler, TimeSpan.FromMinutes(5) },
        { MCXenoHiveCollapseType.Silo, TimeSpan.FromMinutes(5) },
    };
}

[DataDefinition, Serializable, NetSerializable]
public sealed partial class MCXenoHiveConfigEvolution
{
    [DataField]
    public bool WithoutRuler;

    [DataField]
    public HashSet<EntProtoId> BlockedCastes = new();

    [DataField]
    public Dictionary<EntProtoId, int> RequiredCasteCount = new();
}

public enum MCXenoHiveCollapseType
{
    Silo,
    Ruler,
}
