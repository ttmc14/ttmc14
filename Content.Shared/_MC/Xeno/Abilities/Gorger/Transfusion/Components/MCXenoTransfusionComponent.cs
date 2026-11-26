using Robust.Shared.Audio;
using Robust.Shared.GameStates;

namespace Content.Shared._MC.Xeno.Abilities.Gorger.Transfusion.Components;

/// <summary>
/// Grants a channeled heal to a living hive member.
/// Tracks the recipient's health during the channel to detect interruptions.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
[Access(typeof(MCXenoTransfusionSystem))]
public sealed partial class MCXenoTransfusionComponent : Component
{
    /// <summary>
    /// Fraction of the recipient's maximum health supplied by a completed transfusion.
    /// </summary>
    [DataField]
    public float HealFraction = 0.3f;

    /// <summary>
    /// Recipient health at the last channel check, used to detect fresh injuries.
    /// </summary>
    [DataField]
    public float PreviousHealth;

    /// <summary>
    /// Whether the recipient was already critical at the last channel check.
    /// </summary>
    [DataField]
    public bool WasCritical;

    /// <summary>
    /// Maximum distance in tiles between donor and recipient during the transfusion.
    /// </summary>
    [DataField]
    public float Range = 3;

    /// <summary>
    /// Time required to complete a transfusion and spend its blood cost.
    /// </summary>
    [DataField]
    public TimeSpan Delay = TimeSpan.FromSeconds(1);

    /// <summary>
    /// Recipient at the other end of the blood beam while the channel is running.
    /// </summary>
    [DataField, AutoNetworkedField]
    public EntityUid? Recipient;

    /// <summary>
    /// Sound heard by nearby players when blood reaches the recipient.
    /// </summary>
    [DataField]
    public SoundSpecifier TransferSound = new SoundCollectionSpecifier("XenoDrool", AudioParams.Default.WithVolume(-5));
}
