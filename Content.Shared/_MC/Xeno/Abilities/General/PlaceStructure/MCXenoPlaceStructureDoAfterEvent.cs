using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Shared._MC.Xeno.Abilities.General.PlaceStructure;

[Serializable, NetSerializable]
public sealed partial class MCXenoPlaceStructureDoAfterEvent : SimpleDoAfterEvent
{
    public readonly MCXenoPlaceStructurePayload Structure;

    public MCXenoPlaceStructureDoAfterEvent(MCXenoPlaceStructurePayload structure)
    {
        Structure = structure;
    }
}
