using Content.Shared._MC.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Shared._MC.Xeno.Abilities.Gorger.Devour.Events;

[Serializable, NetSerializable]
public sealed partial class MCXenoDevourDoAfterEvent : MCActionSimpleDoAfterEvent
{
    public MCXenoDevourDoAfterEvent(EntityUid action, EntityManager entityManager) : base(action, entityManager)
    {
    }
}
