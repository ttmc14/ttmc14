using Content.Shared._MC.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Shared._MC.Xeno.Abilities.Gorger.Transfusion.Events;

[Serializable, NetSerializable]
public sealed partial class MCXenoTransfusionDoAfterEvent : MCActionSimpleDoAfterEvent
{
    public MCXenoTransfusionDoAfterEvent(EntityUid action, EntityManager entityManager) : base(action, entityManager)
    {
    }
}
