using Content.Shared._MC.DoAfter;
using Content.Shared.DoAfter;
using Robust.Shared.Map;
using Robust.Shared.Serialization;

namespace Content.Shared._MC.Xeno.Abilities.Warlock.Blast;

[Serializable, NetSerializable]
public sealed partial class MCXenoBlastDoAfterEvent : MCActionSimpleDoAfterEvent
{
    public readonly MapCoordinates Start;
    public readonly MapCoordinates Target;

    public MCXenoBlastDoAfterEvent(EntityUid actionUid, MapCoordinates start, MapCoordinates target, EntityManager entityManager) : base(actionUid, entityManager)
    {
        Start = start;
        Target = target;
    }
}
