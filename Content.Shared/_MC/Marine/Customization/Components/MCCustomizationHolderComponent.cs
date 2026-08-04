using Robust.Shared.GameStates;

namespace Content.Shared._MC.Marine.Customization.Components;

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
