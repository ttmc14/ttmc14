using Content.Shared.PowerCell;
using Content.Shared.PowerCell.Components;

namespace Content.Shared._MC.Electrical.PowerCell.Legacy;

public abstract class MCPowerCellProviderSharedSystem : EntitySystem
{
    public abstract bool TryUseActivatableCharge(Entity<PowerCellDrawComponent?, PowerCellSlotComponent?> entity, EntityUid? user = null);

    public abstract bool HasActivatableCharge(Entity<PowerCellDrawComponent?, PowerCellSlotComponent?> entity, EntityUid? user = null);
}
