using Content.Shared._MC.Xeno.Blessings.Prototypes;
using Content.Shared.DoAfter;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._MC.Xeno.Construction.Blessings.Events;

[Serializable, NetSerializable]
public sealed partial class MCXenoBlessingsBuildDoAfterEvent : SimpleDoAfterEvent
{
    public readonly ProtoId<MCXenoBlessingsEntryPrototype> Id;
    public readonly NetCoordinates Coordinates;

    public MCXenoBlessingsBuildDoAfterEvent(ProtoId<MCXenoBlessingsEntryPrototype> id, NetCoordinates coordinates)
    {
        Id = id;
        Coordinates = coordinates;
    }
}
