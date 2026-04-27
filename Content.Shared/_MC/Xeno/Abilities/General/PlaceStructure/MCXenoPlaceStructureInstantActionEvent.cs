using Content.Shared.Actions;
using Robust.Shared.Prototypes;

namespace Content.Shared._MC.Xeno.Abilities.General.PlaceStructure;

public sealed partial class MCXenoPlaceStructureInstantActionEvent : InstantActionEvent
{
    [DataField]
    public MCXenoPlaceStructurePayload Structure;
}

[DataDefinition, Serializable]
public partial struct MCXenoPlaceStructurePayload
{
    [DataField]
    public EntProtoId StructureProtoId;

    [DataField]
    public float PlasmaCost;

    [DataField]
    public TimeSpan Delay;

    [DataField]
    public bool RequireWeeds;
}
