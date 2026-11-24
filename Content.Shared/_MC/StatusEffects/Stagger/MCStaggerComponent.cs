using Robust.Shared.GameStates;

namespace Content.Shared._MC.StatusEffects.Stagger;

/// <summary>
/// Applies the shooter's stagger penalties while this status effect is active.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class MCStaggerComponent : Component
{
    public readonly List<EntityUid> DisabledActions = [];

    [DataField]
    public Angle AdditionalSpread = Angle.FromDegrees(10);

    [DataField]
    public float DamageMultiplier = 0.5f;
}
