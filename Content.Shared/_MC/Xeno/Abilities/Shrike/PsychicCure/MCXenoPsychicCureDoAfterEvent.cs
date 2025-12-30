using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Shared._MC.Xeno.Abilities.Shrike.PsychicCure;

[Serializable, NetSerializable]
public sealed partial class MCXenoPsychicCureDoAfterEvent : SimpleDoAfterEvent
{
    public readonly NetEntity Action;

    public MCXenoPsychicCureDoAfterEvent(NetEntity action)
    {
        Action = action;
    }
}
