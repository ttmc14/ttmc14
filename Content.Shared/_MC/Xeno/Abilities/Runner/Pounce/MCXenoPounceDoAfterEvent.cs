using Content.Shared._MC.DoAfter;
using Robust.Shared.Map;
using Robust.Shared.Serialization;

namespace Content.Shared._MC.Xeno.Abilities.Runner.Pounce;

[Serializable, NetSerializable]
public sealed partial class MCXenoPounceDoAfterEvent : MCActionSimpleDoAfterEvent
{
    public readonly NetCoordinates Coordinates;

    public MCXenoPounceDoAfterEvent(EntityUid actionUid, EntityCoordinates targetCoordinates, EntityManager entityManager) : base(actionUid, entityManager)
    {
        Coordinates = entityManager.GetNetCoordinates(targetCoordinates);
    }
}
