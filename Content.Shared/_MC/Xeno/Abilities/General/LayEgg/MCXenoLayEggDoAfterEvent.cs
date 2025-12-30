using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Shared._MC.Xeno.Abilities.General.LayEgg;

[Serializable, NetSerializable]
public sealed partial class MCXenoLayEggDoAfterEvent : DoAfterEvent
{
    public override DoAfterEvent Clone()
    {
        return this;
    }
}
