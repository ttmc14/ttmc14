using Robust.Shared.Serialization;

namespace Content.Shared._MC.Weapons.Range.Delayed.Events;

[Serializable, NetSerializable]
public sealed class MCWeaponRangeDelayedRequestStopEvent : EntityEventArgs
{
    public NetEntity Gun;
}
