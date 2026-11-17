using Content.Shared._MC.Power.Battery.Components;
using Robust.Client.GameObjects;

namespace Content.Client._MC.Power.Battery;

public sealed class MCBatteryVisualizerSystem : VisualizerSystem<MCBatteryVisualsComponent>
{
    protected override void OnAppearanceChange(EntityUid uid,
        MCBatteryVisualsComponent component,
        ref AppearanceChangeEvent args)
    {
        if (!AppearanceSystem.TryGetData<int>(uid, MCBatteryVisuals.Layer, out var level, args.Component))
            return;

        if (!SpriteSystem.LayerMapTryGet(uid, component.Layer, out var layerId, true))
            return;

        SpriteSystem.LayerSetRsiState(uid, layerId, $"{component.Prefix}{level}");
    }
}
