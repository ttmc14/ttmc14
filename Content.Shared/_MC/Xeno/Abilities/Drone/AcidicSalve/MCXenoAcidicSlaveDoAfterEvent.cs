using Content.Shared._MC.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Shared._MC.Xeno.Abilities.Drone.AcidicSalve;

[Serializable, NetSerializable]
public sealed partial class MCXenoAcidicSlaveDoAfterEvent : MCActionSimpleDoAfterEvent
{
    public MCXenoAcidicSlaveDoAfterEvent(EntityUid actionUid, EntityManager entityManager) : base(actionUid, entityManager)
    {
    }
}
