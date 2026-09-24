using Content.Shared._MC.DoAfter;
using Robust.Shared.Map;
using Robust.Shared.Serialization;

namespace Content.Shared._MC.Xeno.Abilities.Forger.Whirlwind.Events;

[Serializable, NetSerializable]
public sealed partial class MCXenoWhirlwindDoAfterEvent : MCActionSimpleDoAfterEvent
{
    [DataField]
    public NetCoordinates Coordinates;

    public MCXenoWhirlwindDoAfterEvent(EntityUid actionUid, EntityCoordinates coordinates, EntityManager entityManager) : base(actionUid, entityManager)
    {
        Coordinates = entityManager.GetNetCoordinates(coordinates);
    }
}
