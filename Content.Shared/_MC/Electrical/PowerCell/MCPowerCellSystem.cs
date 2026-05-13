using Content.Shared._MC.Electrical.PowerCell.Components;
using Content.Shared._MC.Electrical.PowerCell.Events;
using JetBrains.Annotations;

namespace Content.Shared._MC.Electrical.PowerCell;

public sealed class MCPowerCellSystem : EntitySystem
{
    [PublicAPI]
    public bool HasCharge(Entity<MCPowerCellComponent?> entity, float charge)
    {
        if (!Resolve(entity, ref entity.Comp))
            return false;

        return entity.Comp.Charge >= charge;
    }

    [PublicAPI]
    public bool TryUseCharge(Entity<MCPowerCellComponent?> entity, float value)
    {
        if (!Resolve(entity, ref entity.Comp))
            return false;

        if (value > entity.Comp.Charge)
            return false;

        UseCharge(entity, value);
        return true;
    }

    [PublicAPI]
    public float UseCharge(Entity<MCPowerCellComponent?> entity, float value)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(value, 0);

        if (!Resolve(entity, ref entity.Comp))
            return 0;

        if (entity.Comp.Charge == 0)
            return 0;

        return ChangeCharge(entity, -value);
    }

    [PublicAPI]
    public float ChangeCharge(Entity<MCPowerCellComponent?> entity, float value)
    {
        const float minCharge = 0f;

        if (!Resolve(entity, ref entity.Comp))
            return 0;

        var newValue = float.Clamp(minCharge, entity.Comp.Charge + value, entity.Comp.MaxCharge);
        var delta = newValue - entity.Comp.Charge;

        entity.Comp.Charge = newValue;

        var ev = new MCChargeChangedEvent(entity.Comp.Charge, entity.Comp.MaxCharge);
        RaiseLocalEvent(entity, ref ev);

        return delta;
    }
}
