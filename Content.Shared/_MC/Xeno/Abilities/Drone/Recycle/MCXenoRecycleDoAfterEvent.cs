using Content.Shared._MC.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Shared._MC.Xeno.Abilities.Drone.Recycle;

[Serializable, NetSerializable]
public sealed partial class MCXenoRecycleDoAfterEvent : MCActionSimpleDoAfterEvent
{
    public MCXenoRecycleDoAfterEvent(EntityUid actionUid, EntityManager entityManager) : base(actionUid, entityManager)
    {
    }
}
