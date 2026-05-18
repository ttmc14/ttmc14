using System.Numerics;
using Robust.Shared.GameStates;

namespace Content.Shared._MC.MarkerEye.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MCMarkerEyeOriginComponent : Component
{
    [DataField, AutoNetworkedField]
    public EntityUid? Eye;

    [DataField, AutoNetworkedField]
    public Vector2 OriginalZoom = Vector2.One;

    [DataField, AutoNetworkedField]
    public float OriginalPvsScale = 1f;
}
