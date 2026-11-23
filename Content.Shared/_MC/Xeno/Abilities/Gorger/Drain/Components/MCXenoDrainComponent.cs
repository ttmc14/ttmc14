using Robust.Shared.Audio;
using Robust.Shared.GameStates;

namespace Content.Shared._MC.Xeno.Abilities.Gorger.Drain.Components;

/// <summary>
/// Allows a xenomorph to feed on a living human or a corpse.
/// Only a living victim replenishes blood reserves.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
[Access(typeof(MCXenoDrainSystem))]
public sealed partial class MCXenoDrainComponent : Component
{
    /// <summary>
    /// Units added to the caster's reserves per completed drain of a living victim.
    /// </summary>
    [DataField]
    public float BloodGain = 75;

    /// <summary>
    /// Whether the caster is holding a living victim and cannot attack.
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool Feeding;

    /// <summary>
    /// Number of completed drains in the current hold.
    /// </summary>
    [DataField]
    public int Ticks;

    /// <summary>
    /// Maximum distance in tiles between the caster and the human being drained.
    /// </summary>
    [DataField]
    public float Range = 1.5f;

    /// <summary>
    /// Time required to draw one portion of blood from a living human.
    /// </summary>
    [DataField]
    public TimeSpan Delay = TimeSpan.FromSeconds(1);

    /// <summary>
    /// Interval between healing pulses while feeding on a corpse.
    /// </summary>
    [DataField]
    public TimeSpan CorpseDelay = TimeSpan.FromSeconds(2);

    /// <summary>
    /// Brute damage dealt to a living victim per completed drain.
    /// </summary>
    [DataField]
    public float Damage = 4;

    /// <summary>
    /// Health restored to the caster per completed drain of a living victim.
    /// </summary>
    [DataField]
    public float HealthGain = 40;

    /// <summary>
    /// Units drawn from the victim's bloodstream per completed drain.
    /// </summary>
    [DataField]
    public float BloodDrain = 15;

    /// <summary>
    /// Number of times a single hold can drain a living victim.
    /// </summary>
    [DataField]
    public int DrainTicks = 2;

    /// <summary>
    /// Multiplier applied to wound regeneration when feeding on a corpse.
    /// </summary>
    [DataField]
    public float CorpseHealMultiplier = 2.2f;

    /// <summary>
    /// Carapace condition restored per corpse-feeding pulse, in percentage points.
    /// </summary>
    [DataField]
    public float SunderRecovery = 2.5f;

    /// <summary>
    /// Time before another drain can start after releasing a living victim.
    /// </summary>
    [DataField]
    public TimeSpan Cooldown = TimeSpan.FromSeconds(15);

    /// <summary>
    /// Claw impact at the victim on each successful feeding strike.
    /// </summary>
    [DataField]
    public SoundSpecifier HitSound = new SoundCollectionSpecifier("AlienClaw", AudioParams.Default.WithVolume(-6));
}
