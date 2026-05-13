using Content.Server.PowerCell;
using Content.Shared._MC.Electrical.PowerCell.Legacy;
using Content.Shared.PowerCell;
using Content.Shared.PowerCell.Components;

namespace Content.Server._MC.Electrical.PowerCell.Legacy;

public sealed class MCPowerCellProviderSystem : MCPowerCellProviderSharedSystem
{
    [Dependency] private readonly PowerCellSystem _powerCell = null!;

    public override bool TryUseActivatableCharge(Entity<PowerCellDrawComponent?, PowerCellSlotComponent?> entity, EntityUid? user = null)
    {
        return _powerCell.TryUseActivatableCharge(entity, entity.Comp1, entity.Comp2, user);
    }

    public override bool HasActivatableCharge(Entity<PowerCellDrawComponent?, PowerCellSlotComponent?> entity, EntityUid? user = null)
    {
        return _powerCell.HasActivatableCharge(entity, entity.Comp1, entity.Comp2, user);
    }
}
