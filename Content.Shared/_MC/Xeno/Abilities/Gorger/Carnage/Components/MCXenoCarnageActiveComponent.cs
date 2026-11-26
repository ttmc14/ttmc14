using Robust.Shared.GameStates;

namespace Content.Shared._MC.Xeno.Abilities.Gorger.Carnage.Components;

/// <summary>
/// Holds the strength and expiry of the current Carnage attack.
/// Feeding blocks further attacks until the victim is released.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState, AutoGenerateComponentPause]
[Access(typeof(MCXenoCarnageSystem))]
public sealed partial class MCXenoCarnageActiveComponent : Component
{
    /// <summary>
    /// Strength of this activation, calculated from missing blood before the attack.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float Power;

    /// <summary>
    /// Movement speed multiplier for the duration of this activation.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float SpeedMultiplier;

    /// <summary>
    /// Whether the slash has connected and the caster is holding the victim to draw blood.
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool Feeding;

    /// <summary>
    /// Game time at which an unused Carnage strike expires.
    /// </summary>
    [DataField, AutoNetworkedField, AutoPausedField]
    public TimeSpan EndsAt;
}
