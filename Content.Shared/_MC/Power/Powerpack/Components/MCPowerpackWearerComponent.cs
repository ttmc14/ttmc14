namespace Content.Shared._MC.Power.Powerpack.Components;

/// <summary>
/// Routes held laser notifications to the wearer's equipped recharge packs.
/// </summary>
[RegisterComponent]
public sealed partial class MCPowerpackWearerComponent : Component
{
    public readonly HashSet<EntityUid> Packs = [];
}
