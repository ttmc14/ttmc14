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
    public List<MCLoadoutItem>? Contains = new();

    public override string ToString()
    {
        if (Contains is not { Count: > 0 })
            return $"ProtoId: {ProtoId}";

        var containsString = string.Join(", ", Contains);
        return $"ProtoId: {ProtoId}, Contains: [{containsString}]";
    }
}
