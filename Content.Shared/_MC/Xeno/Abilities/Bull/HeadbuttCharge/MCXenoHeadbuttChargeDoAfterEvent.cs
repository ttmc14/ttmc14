using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Shared._MC.Xeno.Abilities.Bull.HeadbuttCharge;

[Serializable, NetSerializable]
public sealed partial class MCXenoHeadbuttChargeDoAfterEvent : SimpleDoAfterEvent
{
    public readonly NetEntity Action;

    public MCXenoHeadbuttChargeDoAfterEvent(NetEntity action)
    {
        Action = action;
    }
}
