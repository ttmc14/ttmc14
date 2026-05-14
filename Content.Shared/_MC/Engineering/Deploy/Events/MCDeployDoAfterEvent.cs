using Content.Shared.DoAfter;
using Robust.Shared.Map;
using Robust.Shared.Serialization;

namespace Content.Shared._MC.Engineering.Deploy.Events;

[Serializable, NetSerializable]
public sealed partial class MCDeployDoAfterEvent : SimpleDoAfterEvent
{
    public readonly NetCoordinates Coordinates;
    public readonly Angle Angle;

    public MCDeployDoAfterEvent(NetCoordinates coordinates, Angle angle)
    {
        Coordinates = coordinates;
        Angle = angle;
    }
}
