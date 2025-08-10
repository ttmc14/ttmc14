using Content.Shared.DoAfter;
using Robust.Shared.Map;
using Robust.Shared.Serialization;

namespace Content.Shared._MC.Xeno.Abilities.ScatterSpit;

[Serializable, NetSerializable]
public sealed partial class MCXenoScatterSpitDoAfterEvent : SimpleDoAfterEvent
{
    public readonly NetCoordinates Coordinates;
    public readonly NetEntity? Entity;

    public MCXenoScatterSpitDoAfterEvent(NetCoordinates coordinates, NetEntity? entity)
    {
        Coordinates = coordinates;
        Entity = entity;
    }
}
