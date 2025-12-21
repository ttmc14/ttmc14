using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Shared._MC.Xeno.Abilities.Defiler.EmitNeurogas;

[Serializable, NetSerializable]
public sealed partial class MCXenoEmitNeurogasDoAfterEvent : SimpleDoAfterEvent
{
    public readonly NetEntity Action;

    public MCXenoEmitNeurogasDoAfterEvent(NetEntity action)
    {
        Action = action;
    }
}
