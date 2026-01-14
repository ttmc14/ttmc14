using Content.Shared._MC.DoAfter;
using Robust.Shared.Map;
using Robust.Shared.Serialization;

namespace Content.Shared._MC.Xeno.Abilities.Boiler.Bombard;

[Serializable, NetSerializable]
public sealed partial class MCXenoBombardLaunchDoAfter : MCActionSimpleDoAfterEvent
{
    public readonly NetCoordinates TargetCoordinates;
    public readonly NetEntity? TargetUid;

    public MCXenoBombardLaunchDoAfter(EntityUid actionUid, EntityCoordinates coordinates, EntityUid? targetUid, EntityManager entityManager) : base(actionUid, entityManager)
    {
        TargetCoordinates = entityManager.GetNetCoordinates(coordinates);
        TargetUid = entityManager.GetNetEntity(targetUid);
    }
}
