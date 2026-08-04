using Robust.Shared.Serialization;
using Robust.Shared.Utility;

namespace Content.Shared._MC.Marine.Customization;

[DataDefinition, Serializable, NetSerializable]
public partial struct MCCustomizationVariationData
{
    [DataField]
    public string Name = string.Empty;

    [DataField]
    public string Group = string.Empty;

    [DataField]
    public ResPath Path = ResPath.Empty;

    [DataField]
    public string State = "icon";
}
