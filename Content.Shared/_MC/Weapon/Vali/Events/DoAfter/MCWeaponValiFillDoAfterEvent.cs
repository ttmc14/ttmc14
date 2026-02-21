using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Shared._MC.Weapon.Vali.Events.DoAfter;

[Serializable, NetSerializable]
public sealed partial class MCWeaponValiFillDoAfterEvent : SimpleDoAfterEvent
{
    public readonly NetEntity UsedUid;

    public MCWeaponValiFillDoAfterEvent(EntityUid usedUid, EntityManager entityManager)
    {
        UsedUid = entityManager.GetNetEntity(usedUid);
    }
}
