using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._MC.MarkerEye.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class MCMarkerEyeStartOnInteractComponent : Component
{
    [DataField]
    public EntProtoId EyePrototype;
}
