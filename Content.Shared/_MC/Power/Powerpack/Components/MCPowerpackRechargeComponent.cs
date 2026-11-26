using Content.Shared._MC.Power.Battery.Components;
using Robust.Shared.GameStates;

namespace Content.Shared._MC.Power.Powerpack.Components;

/// <summary>
/// Allows this pack to recharge cells in its wearer's held lasers while equipped on the back.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class MCPowerpackRechargeComponent : Component
{
    public EntityUid? Wearer;
    public readonly Dictionary<EntityUid, Entity<MCBatteryComponent>> Cells = [];
}
