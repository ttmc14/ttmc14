using Content.Shared._MC.Xeno.Hive.Prototypes;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._MC.Xeno.Hive.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(fieldDeltas: true)]
public sealed partial class MCXenoHiveComponent : Component
{
    [DataField, AutoNetworkedField]
    public Dictionary<ProtoId<MCXenoHivePsypointTypePrototype>, int> Psypoints = new();

    [DataField, AutoNetworkedField]
    public List<EntityUid> Rulers = new();

    [DataField, AutoNetworkedField]
    public int LarvaPoints;

    [DataField, AutoNetworkedField]
    public int LarvaPointsPerBurrowedLarva = 8;

    [DataField, AutoNetworkedField]
    public MCXenoHiveConfiguration Configuration = new();

    #region Game mod configuration

    [DataField, AutoNetworkedField]
    public TimeSpan RespawnTime = TimeSpan.FromMinutes(3);

    [DataField, AutoNetworkedField]
    public TimeSpan CasteSwapTime = TimeSpan.FromMinutes(5);

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
