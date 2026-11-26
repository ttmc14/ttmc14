using Content.Shared._MC.Power.Battery.Components;
using Robust.Shared.GameStates;

namespace Content.Shared._MC.Power.Powerpack.Components;

/// <summary>
/// Allows this pack to recharge cells in its wearer's held lasers while equipped on the back.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
[Access(typeof(MCPowerpackRechargeSystem))]
public sealed partial class MCPowerpackRechargeComponent : Component
{
    [ViewVariables(VVAccess.ReadWrite), AutoNetworkedField]
    public EntityUid? Wearer;

    [ViewVariables(VVAccess.ReadWrite)]
    public Dictionary<EntityUid, Entity<MCBatteryComponent>> Cells = [];
}
