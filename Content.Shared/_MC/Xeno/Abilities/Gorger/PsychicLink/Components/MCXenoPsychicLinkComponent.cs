using Content.Shared._MC.Aura;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._MC.Xeno.Abilities.Gorger.PsychicLink.Components;

/// <summary>
/// Connects the xenomorph to one hive member after a channel.
/// The protector can absorb injuries down to a minimum health threshold.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
[Access(typeof(MCXenoPsychicLinkSystem))]
public sealed partial class MCXenoPsychicLinkComponent : Component
{
    /// <summary>
    /// Hive member currently protected by the caster, including when temporarily out of range.
    /// </summary>
    [DataField, AutoNetworkedField]
    public EntityUid? Target;

    /// <summary>
    /// Maximum distance in tiles for establishing a link and redirecting damage.
    /// </summary>
    [DataField]
    public float Range = 7;

    /// <summary>
    /// Time required to establish a psychic link.
    /// </summary>
    [DataField]
    public TimeSpan LinkDelay = TimeSpan.FromSeconds(10);

    /// <summary>
    /// Time required to sever a link manually.
    /// </summary>
    [DataField]
    public TimeSpan UnlinkDelay = TimeSpan.FromSeconds(1);

    /// <summary>
    /// Fraction of maximum health the protector must retain when intercepting damage.
    /// </summary>
    [DataField]
    public float MinimumHealth = 0.2f;

    /// <summary>
    /// Fraction of incoming brute and burn damage that the protector intercepts.
    /// </summary>
    [DataField]
    public float RedirectFraction = 0.5f;

    /// <summary>
    /// Time before the caster can establish another link after this one ends.
    /// </summary>
    [DataField]
    public TimeSpan Cooldown = TimeSpan.FromSeconds(15);

    /// <summary>
    /// Psychic confirmation heard when the link is established.
    /// </summary>
    [DataField]
    public SoundSpecifier LinkSound = new SoundPathSpecifier("/Audio/_MC/Effects/magic.ogg", AudioParams.Default.WithVolume(-6));

    /// <summary>
    /// Outline shared by both partners while the link is in range.
    /// </summary>
    [DataField]
    public ProtoId<MCAuraPrototype> Aura = "MCPsychicLinkAura";
}
