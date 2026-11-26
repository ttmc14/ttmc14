using Content.Shared._RMC14.Marines.Squads;
using Robust.Shared.Serialization;

namespace Content.Shared._RMC14.TacticalMap;

[Serializable, NetSerializable]
public enum TacticalMapUserUi
{
    Key,
}

[Serializable, NetSerializable]
public sealed class TacticalMapBuiState(string mapName, Dictionary<int, NetEntity> layers, Dictionary<SquadObjectiveType, string>? squadObjectives = null) : BoundUserInterfaceState
{
    public readonly Dictionary<int, NetEntity> Layers = layers;
    public readonly string MapName = mapName;
    public readonly Dictionary<SquadObjectiveType, string>? SquadObjectives = squadObjectives;
}

[Serializable, NetSerializable]
public sealed class TacticalMapSelectLayerMsg(NetEntity map) : BoundUserInterfaceMessage
{
    public readonly NetEntity Map = map;
}

[Serializable, NetSerializable]
public sealed class TacticalMapLayerDataMsg(
    NetEntity map,
    Dictionary<int, TacticalMapBlip> blips,
    List<TacticalMapLine> lines,
    Dictionary<Vector2i, string> labels) : BoundUserInterfaceMessage
{
    public readonly NetEntity Map = map;
    public readonly Dictionary<int, TacticalMapBlip> Blips = blips;
    public readonly List<TacticalMapLine> Lines = lines;
    public readonly Dictionary<Vector2i, string> Labels = labels;
}
