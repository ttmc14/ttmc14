using Content.Shared.DoAfter;
using Robust.Shared.Map;
using Robust.Shared.Serialization;

namespace Content.Shared._MC.Xeno.Abilities.Wraith.Blink;

[Serializable, NetSerializable]
public sealed partial class MCXenoBlinkDoAfterEvent : DoAfterEvent
{
    public readonly MapCoordinates OriginCoordinates;
    public readonly MapCoordinates TargetCoordinates;
    public readonly NetEntity Action;

    public MCXenoBlinkDoAfterEvent(MapCoordinates originCoordinates, MapCoordinates targetCoordinates, NetEntity action)
    {
        OriginCoordinates = originCoordinates;
        TargetCoordinates = targetCoordinates;
        Action = action;
    }

    public override DoAfterEvent Clone()
    {
        return this;
    }
}
