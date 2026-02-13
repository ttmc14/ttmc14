using Content.Shared.DoAfter;
using Robust.Shared.Map;
using Robust.Shared.Serialization;

namespace Content.Shared._MC.Xeno.Abilities.Warlock.PsyCrush;

[Serializable, NetSerializable]
public sealed partial class MCXenoPsyCrushDoAfterEvent : SimpleDoAfterEvent
{
    public readonly NetCoordinates Coordinates;

    public MCXenoPsyCrushDoAfterEvent(NetCoordinates coordinates)
    {
        Coordinates = coordinates;
    }
}
