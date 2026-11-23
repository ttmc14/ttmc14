using Robust.Shared.GameStates;
using Robust.Shared.Map;

namespace Content.Shared._MC.ASRS.Components;

/// <summary>
/// Stores a dispatched crate's landing coordinates and the time it arrives.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MCASRSDroppedComponent : Component
{
    [DataField, AutoNetworkedField]
    public EntityCoordinates TargetCoordinates;

    [DataField, AutoNetworkedField]
    public EntityUid? EffectUid;

    [DataField, AutoNetworkedField]
    public TimeSpan DropTime;
}
