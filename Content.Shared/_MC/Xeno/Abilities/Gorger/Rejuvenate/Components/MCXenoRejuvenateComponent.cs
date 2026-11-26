using Content.Shared._MC.Aura;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._MC.Xeno.Abilities.Gorger.Rejuvenate.Components;

/// <summary>
/// Configures the blood upkeep and healing interval of Rejuvenate.
/// The damage threshold controls when the thickened carapace starts absorbing hits.
/// </summary>
[RegisterComponent, NetworkedComponent]
[Access(typeof(MCXenoRejuvenateSystem))]
public sealed partial class MCXenoRejuvenateComponent : Component
{
    /// <summary>
    /// Blood consumed on activation and again at each healing pulse.
    /// </summary>
    [DataField]
    public float Cost = 20;

    /// <summary>
    /// Time between healing pulses and resets of the incoming damage counter.
    /// </summary>
    [DataField]
    public TimeSpan Interval = TimeSpan.FromSeconds(2);

    /// <summary>
    /// Fraction of maximum health restored to the caster per pulse.
    /// </summary>
    [DataField]
    public float HealFraction = 0.05f;

    /// <summary>
    /// Fraction of maximum health that must be taken as damage in one interval before protection applies.
    /// </summary>
    [DataField]
    public float DamageThreshold = 0.1f;

    /// <summary>
    /// Fraction of each qualifying brute or burn hit absorbed after reaching the threshold.
    /// </summary>
    [DataField]
    public float DamageReduction = 0.75f;

    /// <summary>
    /// Maximum damage absorbed from one qualifying damage type in a hit.
    /// </summary>
    [DataField]
    public float MaxReduction = 40;

    /// <summary>
    /// Movement speed multiplier while the carapace is strengthened.
    /// </summary>
    [DataField]
    public float SpeedMultiplier = 0.35f;

    /// <summary>
    /// Time after activation before the caster can end the effect manually.
    /// </summary>
    [DataField]
    public TimeSpan CancelDelay = TimeSpan.FromSeconds(1);

    /// <summary>
    /// Time before the caster can reactivate Rejuvenate after it ends.
    /// </summary>
    [DataField]
    public TimeSpan Cooldown = TimeSpan.FromSeconds(4);

    /// <summary>
    /// Psychic cue heard when the caster hardens its carapace.
    /// </summary>
    [DataField]
    public SoundSpecifier StartSound = new SoundPathSpecifier("/Audio/_MC/Effects/magic.ogg", AudioParams.Default.WithVolume(-6));

    /// <summary>
    /// Visual preset applied while this effect is active.
    /// </summary>
    [DataField]
    public ProtoId<MCAuraPrototype> Aura = "MCRejuvenateAura";
}
