using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Shared._MC.Xeno.Abilities.Forger.Inferno;

[Serializable, NetSerializable]
public sealed partial class MCXenoInfernoDoAfterEvent : SimpleDoAfterEvent
{
    public readonly NetEntity Action;

    public MCXenoInfernoDoAfterEvent(NetEntity action)
    {
        Action = action;
    }
}
