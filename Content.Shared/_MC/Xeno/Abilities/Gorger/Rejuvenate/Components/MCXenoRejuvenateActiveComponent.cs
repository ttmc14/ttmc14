using Robust.Shared.GameStates;

namespace Content.Shared._MC.Xeno.Abilities.Gorger.Rejuvenate.Components;

/// <summary>
/// Slows the xenomorph while Rejuvenate is active.
/// Tracks incoming damage until the next healing pulse resets the threshold.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState, AutoGenerateComponentPause]
[Access(typeof(MCXenoRejuvenateSystem))]
public sealed partial class MCXenoRejuvenateActiveComponent : Component
{
    /// <summary>
    /// Brute and burn damage received since the last pulse, before this ability's reduction.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float DamageThisTick;

    /// <summary>
    /// Movement speed multiplier captured when the carapace was strengthened.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float SpeedMultiplier;

    /// <summary>
    /// Game time at which the next blood payment and healing pulse occur.
    /// </summary>
    [DataField, AutoNetworkedField, AutoPausedField]
    public TimeSpan NextTick;
}
