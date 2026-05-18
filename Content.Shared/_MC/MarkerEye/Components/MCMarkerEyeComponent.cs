using Robust.Shared.GameStates;

namespace Content.Shared._MC.MarkerEye.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MCMarkerEyeComponent : Component
{
    [DataField, AutoNetworkedField]
    public EntityUid? Origin;
}
