using Content.Shared._MC.Electrical.PowerCell.Legacy;
using Content.Shared.PowerCell;
using Content.Shared.PowerCell.Components;

namespace Content.Client._MC.Electrical.PowerCell.Legacy;

public sealed class MCPowerCellProviderSystem : MCPowerCellProviderSharedSystem
{
    public override bool TryUseActivatableCharge(Entity<PowerCellDrawComponent?, PowerCellSlotComponent?> entity, EntityUid? user = null)
    {
        return false;
    }

    public override bool HasActivatableCharge(Entity<PowerCellDrawComponent?, PowerCellSlotComponent?> entity, EntityUid? user = null)
    {
        return false;
    }
}
