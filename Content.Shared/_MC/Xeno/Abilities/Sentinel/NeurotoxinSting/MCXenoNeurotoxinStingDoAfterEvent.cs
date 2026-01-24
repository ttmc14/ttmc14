using Content.Shared._MC.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Shared._MC.Xeno.Abilities.Sentinel.NeurotoxinSting;

[Serializable, NetSerializable]
public sealed partial class MCXenoNeurotoxinStingDoAfterEvent : MCActionSimpleDoAfterEvent
{
    public int Injects = 0;

    public MCXenoNeurotoxinStingDoAfterEvent(EntityUid actionUid, EntityManager entityManager) : base(actionUid, entityManager)
    {
    }
}
