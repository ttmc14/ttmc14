using Content.Shared._MC.Engineering.Miners.Components;
using Content.Shared._MC.Engineering.Miners.Events.Modules;
using JetBrains.Annotations;

namespace Content.Shared._MC.Engineering.Miners;

public sealed partial class MCMinerSystem
{
    [PublicAPI]
    public void Refresh(Entity<MCMinerComponent?> entity)
    {
        if (!Resolve(entity, ref entity.Comp))
        {
            Log.Warning($"Trying to refresh entity {ToPrettyString(entity)} without {nameof(MCMinerComponent)}. Aborted");
            return;
        }

        var automatedEv = new MCMinerGetAutomatedEvent();
        RaiseLocalEvent(entity, ref automatedEv);

        entity.Comp.MineralStorageAutoSale = automatedEv.Automated;
        DirtyField(entity.Owner, entity.Comp, nameof(MCMinerComponent.MineralStorageAutoSale));

        var mineralValueEv = new MCMinerGetMineralValueEvent(entity.Comp.MineralValue);
        RaiseLocalEvent(entity, ref mineralValueEv);

        entity.Comp.MineralValueTotal = int.Max(0, mineralValueEv.Value);
        DirtyField(entity.Owner, entity.Comp, nameof(MCMinerComponent.MineralStorageAutoSale));

        var productionTimeValue = new MCMinerGetProductionTimeEvent(entity.Comp.MineralProductionTime);
        RaiseLocalEvent(entity, ref productionTimeValue);

        entity.Comp.MineralProductionTimeTotal = productionTimeValue.Value < TimeSpan.Zero ? TimeSpan.Zero : productionTimeValue.Value;
        DirtyField(entity.Owner, entity.Comp, nameof(MCMinerComponent.MineralProductionTimeTotal));
    }
}
