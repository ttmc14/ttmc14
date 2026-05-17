using Robust.Shared.Map;
using Robust.Shared.Serialization;
using Robust.Shared.Timing;

namespace Content.Shared._MC.Weapons.Range.Delayed.Events;

[Serializable, NetSerializable]
public sealed class MCWeaponRangeDelayedRequestStartEvent : EntityEventArgs
{
    public NetEntity Gun;
    public NetCoordinates Coordinates;
    public NetEntity? Target;
    public GameTick LastRealTick;
}
