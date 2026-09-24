using Content.Shared._MC.Engineering.Miners.Components;
using Content.Shared._RMC14.TacticalMap;
using Robust.Shared.Utility;

namespace Content.Shared._MC.Engineering.Miners;

public sealed partial class MCMinerSystem
{
    private void UpdateAppearance(Entity<MCMinerComponent> entity)
    {
        _appearance.SetData(entity, MCMinerLayers.State, entity.Comp.State);
    }

    private void UpdateMapIcon(Entity<MCMinerComponent> entity)
    {
        if (!TryComp<TacticalMapIconComponent>(entity, out var iconComponent) || iconComponent.Icon is not { } icon)
            return;

        var ensure = EnsureComp<MapBlipIconOverrideComponent>(entity);
        var state = entity.Comp.State == MCMinerState.Running ? "phoron-on" : "phoron";
        var newIcon = new SpriteSpecifier.Rsi(icon.RsiPath, state);

        ensure.Icon = newIcon;

        Dirty(entity, ensure);
    }
}
