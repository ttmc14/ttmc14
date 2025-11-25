using Content.Shared.DoAfter;
using Robust.Shared.Map;
using Robust.Shared.Serialization;

namespace Content.Shared._MC.Xeno.Abilities.Fireball;

[Serializable, NetSerializable]
public sealed partial class MCXenoFireballDoAfterEvent : SimpleDoAfterEvent
{
    public readonly NetCoordinates Coordinates;
    public readonly NetEntity? Entity;

    public MCXenoFireballDoAfterEvent(NetCoordinates coordinates, NetEntity? entity)
    {
        Coordinates = coordinates;
        Entity = entity;
    }
}
