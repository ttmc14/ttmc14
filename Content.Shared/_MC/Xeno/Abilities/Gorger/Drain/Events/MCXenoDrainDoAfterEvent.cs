using Content.Shared._MC.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Shared._MC.Xeno.Abilities.Gorger.Drain.Events;

[Serializable, NetSerializable]
public sealed partial class MCXenoDrainDoAfterEvent : MCActionSimpleDoAfterEvent
{
    /// <summary>
    /// Whether the channel started on a corpse and should heal without collecting blood.
    /// </summary>
    public readonly bool Corpse;

    public MCXenoDrainDoAfterEvent(EntityUid action, EntityManager entityManager, bool corpse) : base(action, entityManager)
    {
        Corpse = corpse;
    }
}
