using Content.Shared._MC.Xeno.Hive.Prototypes;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._MC.Xeno.Hive.UI.Status;

[Serializable, NetSerializable]
public sealed partial class MCXenoHiveStatusXenosBuiState : BoundUserInterfaceState
{
    public List<MCXenoEntity> Xenos { get; init; } = new();
    public List<MCXenoStructure> Structures { get; init; } = new();

    public int LarvaPoints { get; init; }

    public bool LarvaGeneration { get; init; }

    public int BurrowedLarva { get; init; }

    public Dictionary<ProtoId<MCXenoHivePsypointTypePrototype>, int> Psypoints { get; init; } = new();

    public Dictionary<int, int> TierSlots { get; init; } = new();

    public bool BlessingsHide { get; init; }

    public bool DevolutionHide { get; init; }
    public bool EvolutionHide { get; init; }

    public int EvolutionPoints { get; init; }

    public int EvolutionPointsMax { get; init; }

    public bool EvolutionPointsHide { get; init; }
}

[Serializable, NetSerializable]
public readonly record struct MCXenoEntity(NetEntity Entity, string Name, int Tier, float Health, float Plasma, EntProtoId? Id);

[Serializable, NetSerializable]
public readonly record struct MCXenoStructure(NetEntity Entity, string Name, float Health, EntProtoId? Id);
