using Robust.Shared.GameStates;

namespace Content.Shared._MC.Xeno.Abilities.Heal.HealingInfusion.Components;

/// <summary>
/// Connects the Hivelord to the recipient during the initial pulse of Healing Infusion.
/// The beam entity is attached to the caster for its lifetime.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
[Access(typeof(MCXenoHealingInfusionSystem))]
public sealed partial class MCXenoHealingInfusionBeamComponent : Component
{
    /// <summary>
    /// Living hive member at the far end of the healing beam.
    /// </summary>
    [DataField, AutoNetworkedField]
    public EntityUid? Recipient;

    /// <summary>
    /// Maximum distance in tiles at which the initial pulse remains visible.
    /// </summary>
    [DataField]
    public float Range = 10;
}
