using Content.Shared._MC.Aura;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._MC.Xeno.Abilities.Gorger.Carnage.Components;

/// <summary>
/// Grants a burst of speed and healing on the next slash against a living human.
/// Missing blood at activation determines the strength of the effect.
/// </summary>
[RegisterComponent, NetworkedComponent]
[Access(typeof(MCXenoCarnageSystem))]
public sealed partial class MCXenoCarnageComponent : Component
{
    /// <summary>
    /// Blood added after a successful feeding strike, multiplied by the power captured at activation.
    /// </summary>
    [DataField]
    public float BloodGain = 150;

    /// <summary>
    /// Fraction of the caster's maximum health restored by a full-power slash.
    /// </summary>
    [DataField]
    public float HealFraction = 0.2f;

    /// <summary>
    /// Minimum power at which a slash knocks the victim down and starts feeding.
    /// </summary>
    [DataField]
    public float HighThirst = 0.8f;

    /// <summary>
    /// Movement speed bonus at full power, expressed as a fraction of base speed.
    /// </summary>
    [DataField]
    public float SpeedBonus = 0.5f;

    /// <summary>
    /// Units of blood removed from the victim after a completed feeding strike.
    /// </summary>
    [DataField]
    public float BloodDrain = 30;

    /// <summary>
    /// Maximum distance in tiles between the caster and the victim while feeding.
    /// </summary>
    [DataField]
    public float Range = 1.5f;

    /// <summary>
    /// Radius in tiles in which a Carnage strike also heals hive members during Feast.
    /// </summary>
    [DataField]
    public float HealRange = 4;

    /// <summary>
    /// Time available to land the empowered slash.
    /// </summary>
    [DataField]
    public TimeSpan Duration = TimeSpan.FromSeconds(10);

    /// <summary>
    /// Time the caster must hold a knocked-down victim to collect blood.
    /// </summary>
    [DataField]
    public TimeSpan FeedDelay = TimeSpan.FromSeconds(1);

    /// <summary>
    /// Roar heard when the caster enters bloodlust.
    /// </summary>
    [DataField]
    public SoundSpecifier RoarSound = new SoundCollectionSpecifier("XenoRoar");

    /// <summary>
    /// Visual preset applied while this effect is active.
    /// </summary>
    [DataField]
    public ProtoId<MCAuraPrototype> Aura = "MCCarnageAura";
}
