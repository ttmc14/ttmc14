using Content.Shared._MC.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Shared._MC.Xeno.Abilities.Forger.Inferno.Events;

[Serializable, NetSerializable]
public sealed partial class MCXenoInfernoDoAfterEvent : MCActionSimpleDoAfterEvent
{
    public MCXenoInfernoDoAfterEvent(EntityUid actionUid, EntityManager entityManager) : base(actionUid, entityManager)
    {

    }
}
