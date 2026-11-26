using Robust.Shared.Audio;
using Robust.Shared.GameStates;

namespace Content.Shared._MC.Xeno.Abilities.Gorger.Oppose.Components;

/// <summary>
/// Grants a stomp that restores nearby hive members and staggers adjacent enemies.
/// Enemies sharing the caster's tile also take brute damage.
/// </summary>
[RegisterComponent, NetworkedComponent]
[Access(typeof(MCXenoOpposeSystem))]
public sealed partial class MCXenoOpposeComponent : Component
{
    /// <summary>
    /// Fraction of each hive member's maximum health restored by the stomp.
    /// </summary>
    [DataField]
    public float HealFraction = 0.2f;

    /// <summary>
    /// Number of tiles the healing pulse reaches from the caster, including diagonals.
    /// </summary>
    [DataField]
    public int HealRange = 3;

    /// <summary>
    /// Brute damage dealt to enemies on the caster's tile.
    /// </summary>
    [DataField]
    public float Damage = 20;

    /// <summary>
    /// Duration passed to the stagger effect on enemies within one tile.
    /// </summary>
    [DataField]
    public TimeSpan StaggerDuration = TimeSpan.FromSeconds(2);

    /// <summary>
    /// Slowdown stacks applied to enemies within one tile.
    /// </summary>
    [DataField]
    public float Slowdown = 3;

    /// <summary>
    /// Ground impact heard at the centre of the stomp.
    /// </summary>
    [DataField]
    public SoundSpecifier StompSound = new SoundPathSpecifier("/Audio/_RMC14/Xeno/alien_footstep_charge1.ogg", AudioParams.Default.WithVolume(-6));
}
