using Content.Shared._MC.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Shared._MC.Xeno.Abilities.Gorger.PsychicLink.Events;

[Serializable, NetSerializable]
public sealed partial class MCXenoPsychicLinkDoAfterEvent : MCActionSimpleDoAfterEvent
{
    /// <summary>
    /// Whether this channel removes the existing link instead of establishing a new one.
    /// </summary>
    public readonly bool Unlink;

    public MCXenoPsychicLinkDoAfterEvent(EntityUid action, EntityManager entityManager, bool unlink) : base(action, entityManager)
    {
        Unlink = unlink;
    }
}
