using Robust.Shared.GameStates;

namespace Content.Shared._MC.Power.Powerpack.Components;

/// <summary>
/// Routes held laser notifications to the wearer's equipped recharge packs.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
[Access(typeof(MCPowerpackRechargeSystem))]
public sealed partial class MCPowerpackWearerComponent : Component
{
    [ViewVariables(VVAccess.ReadWrite), AutoNetworkedField]
    public HashSet<EntityUid> Packs = [];
}
