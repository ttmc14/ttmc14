using Robust.Shared.GameStates;

namespace Content.Shared._MC.Xeno.Abilities.Gorger.Feast.Components;

/// <summary>
/// Schedules the next healing pulse while Feast is active.
/// Retains the original cooldown deadline if the Gorger stops feeding early.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState, AutoGenerateComponentPause]
[Access(typeof(MCXenoFeastSystem))]
public sealed partial class MCXenoFeastActiveComponent : Component
{
    /// <summary>
    /// Game time at which the next pulse consumes blood and heals nearby hive members.
    /// </summary>
    [DataField, AutoNetworkedField, AutoPausedField]
    public TimeSpan NextTick;

    /// <summary>
    /// Earliest game time at which the caster can start another feast.
    /// </summary>
    [DataField, AutoNetworkedField, AutoPausedField]
    public TimeSpan ReadyAt;
}
