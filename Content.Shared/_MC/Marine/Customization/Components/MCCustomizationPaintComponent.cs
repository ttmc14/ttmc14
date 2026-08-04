using Robust.Shared.GameStates;

namespace Content.Shared._MC.Marine.Customization.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(fieldDeltas: true)]
public sealed partial class MCCustomizationPaintComponent : Component
{
    [DataField, AutoNetworkedField]
    public HashSet<string>? AvailableVariations;

    [DataField, AutoNetworkedField]
    public TimeSpan Delay = TimeSpan.FromSeconds(0.5d);
}
