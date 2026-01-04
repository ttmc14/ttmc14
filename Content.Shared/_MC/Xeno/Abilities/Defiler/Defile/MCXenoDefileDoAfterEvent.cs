using Content.Shared._MC.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Shared._MC.Xeno.Abilities.Defiler.Defile;

[Serializable, NetSerializable]
public sealed partial class MCXenoDefileDoAfterEvent : MCActionSimpleDoAfterEvent
{
    public MCXenoDefileDoAfterEvent(EntityUid actionUid, EntityManager entityManager) : base(actionUid, entityManager)
    {
    }
}
