using Content.Shared._MC.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Shared._MC.Xeno.Abilities.Boiler.Bombard;

[Serializable, NetSerializable]
public sealed partial class MCXenoBombardDiggingDoAfter : MCActionSimpleDoAfterEvent
{
    public MCXenoBombardDiggingDoAfter(EntityUid actionUid, EntityManager entityManager) : base(actionUid, entityManager)
    {
    }
}
