using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Shared._MC.Xeno.Abilities.Psydrain;

[Serializable, NetSerializable]
public sealed partial class MCXenoPsydrainDoAfterEvent : SimpleDoAfterEvent
{
    private readonly NetEntity _actionUid;

    public MCXenoPsydrainDoAfterEvent(EntityUid actionNetUid, EntityManager entityManager)
    {
        _actionUid = entityManager.GetNetEntity(actionNetUid);
    }

    public EntityUid GetActionUid(EntityManager entityManager)
    {
        return entityManager.GetEntity(_actionUid);
    }
}
