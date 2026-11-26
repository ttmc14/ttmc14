using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._MC.Xeno.Abilities.Heal.HealingInfusion.Components;

/// <summary>
/// Grants the Hivelord a targeted healing infusion and its initial beam.
/// </summary>
[Access(typeof(MCXenoHealingInfusionSystem))]
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(fieldDeltas: true)]
public sealed partial class MCXenoHealingInfusionComponent : Component
{
    /// <summary>
    /// Status effect applied to a friendly target.
    /// </summary>
    [DataField, AutoNetworkedField]
    public EntProtoId StatusEffectId = "MCStatusEffectXenoHealingInfusion";

    /// <summary>
    /// Duration of the infusion on the target.
    /// </summary>
    [DataField, AutoNetworkedField]
    public TimeSpan StatusEffectDuration = TimeSpan.FromSeconds(60);

    /// <summary>
    /// Beam prototype attached to the Hivelord during the initial pulse.
    /// </summary>
    [DataField, AutoNetworkedField]
    public EntProtoId RayEffectId = "MCEffectTransferMedRay";

    /// <summary>
    /// Effect shown on the recipient when the infusion starts.
    /// </summary>
    [DataField, AutoNetworkedField]
    public EntProtoId EffectProtoId = "RMCEffectHealHealer";

    /// <summary>
    /// Sound played when the infusion is applied.
    /// </summary>
    [DataField, AutoNetworkedField]
    public SoundSpecifier? Sound = new SoundPathSpecifier("/Audio/_MC/Effects/magic.ogg");
}
