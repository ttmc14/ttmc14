using Content.Shared._MC.Aura;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._MC.Xeno.StatusEffects.HealingInfusion;

/// <summary>
/// Tracks the healing and armor recovery granted to the infused xenomorph.
/// </summary>
[Access(typeof(MCXenoStatusEffectHealingInfusionSystem))]
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(fieldDeltas: true)]
public sealed partial class MCXenoStatusEffectHealingInfusionComponent : Component
{
    /// <summary>
    /// Game time of the next healing pulse.
    /// </summary>
    [DataField, AutoNetworkedField]
    public TimeSpan TickNext;

    /// <summary>
    /// Interval between healing pulses.
    /// </summary>
    [DataField, AutoNetworkedField]
    public TimeSpan TickDelay = TimeSpan.FromSeconds(1);

    /// <summary>
    /// Time remaining for wound healing.
    /// </summary>
    [DataField, AutoNetworkedField]
    public TimeSpan HealthDurationRemaining = TimeSpan.FromSeconds(10);

    /// <summary>
    /// Time remaining for armor recovery.
    /// </summary>
    [DataField, AutoNetworkedField]
    public TimeSpan SunderDurationRemaining = TimeSpan.FromSeconds(10);

    /// <summary>
    /// Base wound healing per pulse.
    /// </summary>
    [DataField]
    public float Heal = 6;

    /// <summary>
    /// Visual preset applied while this effect is active.
    /// </summary>
    [DataField]
    public ProtoId<MCAuraPrototype> Aura = "MCHealingInfusionAura";
}
