using Robust.Shared.Audio;
using Robust.Shared.GameStates;

namespace Content.Shared._MC.Xeno.Abilities.Gorger.Devour.Components;

/// <summary>
/// Allows the xenomorph to carry one unrevivable human corpse in its stomach.
/// Targeting itself releases the corpse after a short delay.
/// </summary>
[RegisterComponent, NetworkedComponent]
[Access(typeof(MCXenoDevourSystem))]
public sealed partial class MCXenoDevourComponent : Component
{
    /// <summary>
    /// Maximum distance in tiles for swallowing a corpse and checking for nearby fog.
    /// </summary>
    [DataField]
    public float Range = 1.5f;

    /// <summary>
    /// Time the caster must stay beside the corpse before swallowing it.
    /// </summary>
    [DataField]
    public TimeSpan DevourDelay = TimeSpan.FromSeconds(2);

    /// <summary>
    /// Time needed to release the stored corpse when targeting the caster.
    /// </summary>
    [DataField]
    public TimeSpan RegurgitateDelay = TimeSpan.FromSeconds(1);

    /// <summary>
    /// Swallowing sound played while pulling a corpse into the stomach.
    /// </summary>
    [DataField]
    public SoundSpecifier SwallowSound = new SoundPathSpecifier("/Audio/_MC/Effects/alien_bite2.ogg", AudioParams.Default.WithVolume(-4));

    /// <summary>
    /// Sound played while releasing the corpse.
    /// </summary>
    [DataField]
    public SoundSpecifier ReleaseSound = new SoundPathSpecifier("/Audio/Effects/Fluids/slosh.ogg", AudioParams.Default.WithVolume(-4));

    /// <summary>
    /// Audio entity for the current channel, stopped when it ends or is interrupted.
    /// </summary>
    [DataField]
    public EntityUid? ChannelAudio;
}
