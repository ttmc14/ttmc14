using Content.Shared._MC.Engineering.Miners.Components;

namespace Content.Shared._MC.Engineering.Miners;

public sealed partial class MCMinerSystem
{
    private void UpdateAppearance(Entity<MCMinerComponent> entity)
    {
        _appearance.SetData(entity, MCMinerLayers.State, entity.Comp.State);
    }

    private void UpdateMapIcon(Entity<MCMinerComponent> entity)
    {
        if (entity.Comp.State != MCMinerState.Running)
        {
            _mcTacticalMapIconSwitcher.ResetIcon(entity.Owner);
            return;
        }

        _mcTacticalMapIconSwitcher.SetIcon(entity.Owner, "on");
    }
}
