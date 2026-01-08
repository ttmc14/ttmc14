using Robust.Shared.GameStates;

namespace Content.Shared._MC.Effect;

[RegisterComponent, NetworkedComponent]
public sealed partial class MCTimedDespawnDisappearanceVisualsComponent : Component
{
    [DataField]
    public float Lifetime;

    [DataField]
    public float MinAlpha;

    [DataField]
    public float MaxAlpha = 1f;
}
