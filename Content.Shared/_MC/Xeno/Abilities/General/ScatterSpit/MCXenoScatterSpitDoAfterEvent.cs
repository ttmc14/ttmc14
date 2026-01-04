using Content.Shared.DoAfter;
using Robust.Shared.Map;
using Robust.Shared.Serialization;

namespace Content.Shared._MC.Xeno.Abilities.General.ScatterSpit;

[Serializable, NetSerializable]
public sealed partial class MCXenoScatterSpitDoAfterEvent : SimpleDoAfterEvent
{
    public readonly NetCoordinates Coordinates;
    public readonly NetEntity Action;
    public readonly NetEntity? Entity;

    public MCXenoScatterSpitDoAfterEvent(NetCoordinates coordinates, NetEntity action, NetEntity? entity)
    {
        Coordinates = coordinates;
        Action = action;
        Entity = entity;
    }
}
