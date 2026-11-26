using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._MC.Serialization.Loadout.Data;

[DataDefinition, Serializable, NetSerializable]
public sealed partial class MCLoadoutItem
{
    [DataField(required: true)]
    public EntProtoId ProtoId;

    [DataField]
    public EntProtoId? VendorProtoId;

    [DataField]
    public List<MCLoadoutPart> Parts = new();

    public override string ToString() => ProtoId.Id;
}
