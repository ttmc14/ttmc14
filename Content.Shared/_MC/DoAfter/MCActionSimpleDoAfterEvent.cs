using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Shared._MC.DoAfter;

[Serializable, NetSerializable]
public abstract partial class MCActionSimpleDoAfterEvent : SimpleDoAfterEvent
{
    public readonly NetEntity ActionUid;

    protected MCActionSimpleDoAfterEvent(NetEntity actionUid)
    {
        ActionUid = actionUid;
    }

    protected MCActionSimpleDoAfterEvent(EntityUid actionUid, EntityManager entityManager)
    {
        ActionUid = entityManager.GetNetEntity(actionUid);
    }

    public EntityUid GetAction(EntityManager entityManager)
    {
        return entityManager.GetEntity(ActionUid);
    }
}
