using Robust.Shared.GameStates;

namespace Content.Shared._MC.Xeno.Abilities.Gorger.PsychicLink.Components;

/// <summary>
/// Marks the recipient of a psychic link.
/// Identifies the xenomorph that takes a share of its brute and burn damage.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
[Access(typeof(MCXenoPsychicLinkSystem))]
public sealed partial class MCXenoPsychicLinkTargetComponent : Component
{
    /// <summary>
    /// Xenomorph that receives the redirected portion of this recipient's damage.
    /// </summary>
    [DataField, AutoNetworkedField]
    public EntityUid Protector;
}
