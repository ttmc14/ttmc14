using Content.Shared.DoAfter;
using Robust.Shared.Map;
using Robust.Shared.Serialization;

namespace Content.Shared._MC.Xeno.Abilities.Warlock.Blast;

[Serializable, NetSerializable]
public sealed partial class MCXenoBlastDoAfterEvent : SimpleDoAfterEvent
{
    public readonly MapCoordinates Start;
    public readonly MapCoordinates Target;

    public MCXenoBlastDoAfterEvent(MapCoordinates start, MapCoordinates target)
    {
        Start = start;
        Target = target;
    }
}
