using Robust.Shared.GameStates;
using Robust.Shared.Serialization;
using Robust.Shared.Utility;

namespace Content.Shared._MC.Marine.Customization;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(fieldDeltas: true)]
public sealed partial class MCCustomizationHolderComponent : Component
{
    [DataField]
    public bool Paintable = true;

    [DataField, AutoNetworkedField]
    public string? State;

    [DataField, AutoNetworkedField]
    public Dictionary<string, MCCustomizationVariationData> Variations = new();
}

[DataDefinition, Serializable, NetSerializable]
public partial struct MCCustomizationVariationData
{
    [DataField]
    public string Name = string.Empty;

    [DataField]
    public string Group = string.Empty;

    [DataField]
    public ResPath Path = ResPath.Empty;
}
