using Content.Shared.DoAfter;
using Robust.Shared.Map;
using Robust.Shared.Serialization;

namespace Content.Shared._MC.Xeno.Abilities.Forger.Whirlwind;

[Serializable, NetSerializable]
public sealed partial class MCXenoWhirlwindDoAfter : SimpleDoAfterEvent
{
    [DataField]
    public NetCoordinates Coordinates;

    public MCXenoWhirlwindDoAfter(NetCoordinates coordinates)
    {
        Coordinates = coordinates;
    }
}
