using Content.Shared._MC.Xeno.Visuals.Damage;
using Content.Shared._RMC14.Xenonids;
using Content.Shared._RMC14.Xenonids.Damage;
using Robust.Client.GameObjects;

namespace Content.Client._MC.Xeno.Damage;

public sealed class MCXenoDamageVisualsSystem : VisualizerSystem<MCXenoDamageVisualsComponent>
{
    protected override void OnAppearanceChange(EntityUid uid, MCXenoDamageVisualsComponent component, ref AppearanceChangeEvent args)
    {
        if (!component.Enabled)
            return;

        var sprite = args.Sprite;
        if (sprite is null ||
            !AppearanceSystem.TryGetData(uid, RMCDamageVisuals.State, out int level) ||
            !sprite.LayerMapTryGet(RMCDamageVisualLayers.Base, out var layer))
            return;

        if (level == 0)
        {
            sprite.LayerSetVisible(layer, false);
            return;
        }

        sprite.LayerSetVisible(layer, true);

        var state = component.States - level + 1;

        if (AppearanceSystem.TryGetData(uid, RMCXenoStateVisuals.Downed, out bool downed) && downed)
        {
            sprite.LayerSetState(layer, $"wounded_crit_{state}");
            return;
        }

        if (AppearanceSystem.TryGetData(uid, RMCXenoStateVisuals.Fortified, out bool fortified) && fortified)
        {
            sprite.LayerSetState(layer, $"wounded_fortify_{state}");
            return;
        }

        if (AppearanceSystem.TryGetData(uid, RMCXenoStateVisuals.Resting, out bool resting) && resting)
        {
            sprite.LayerSetState(layer, $"wounded_resting_{state}");
            return;
        }

        sprite.LayerSetState(layer, $"wounded_{state}");
    }
}
