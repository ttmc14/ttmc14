using Content.Shared._MC.Xeno.Visuals.Damage;
using Content.Shared._RMC14.Xenonids;
using Content.Shared._RMC14.Xenonids.Damage;
using Robust.Client.GameObjects;

namespace Content.Client._MC.Xeno.Damage;

public sealed class MCXenoDamageVisualsSystem : VisualizerSystem<MCXenoDamageVisualsComponent>
{
    private const string BaseState = "wounded";
    private const string CritState = $"{BaseState}_crit";
    private const string FortifyState = $"{BaseState}_fortify";
    private const string RestringState = $"{BaseState}_resting";

    protected override void OnAppearanceChange(EntityUid uid, MCXenoDamageVisualsComponent component, ref AppearanceChangeEvent args)
    {
        if (!component.Enabled)
            return;

        if (args.Sprite is not {} sprite)
            return;

        var entity = new Entity<SpriteComponent?>(uid, sprite);
        if (!AppearanceSystem.TryGetData(uid, RMCDamageVisuals.State, out int level))
            return;

        if (!SpriteSystem.LayerMapTryGet(entity, RMCDamageVisualLayers.Base, out var layer, true))
            return;

        if (level == 0)
        {
            SpriteSystem.LayerSetVisible(entity, layer, false);
            return;
        }

        SpriteSystem.LayerSetVisible(entity, layer, true);

        var state = component.States - level + 1;
        if (AppearanceSystem.TryGetData(uid, RMCXenoStateVisuals.Downed, out bool downed) && downed)
        {
            SpriteSystem.LayerSetRsiState(entity, layer, $"{CritState}_{state}");
            return;
        }

        if (AppearanceSystem.TryGetData(uid, RMCXenoStateVisuals.Fortified, out bool fortified) && fortified)
        {
            SpriteSystem.LayerSetRsiState(entity, layer, $"{FortifyState}_{state}");
            return;
        }

        if (AppearanceSystem.TryGetData(uid, RMCXenoStateVisuals.Resting, out bool resting) && resting)
        {
            SpriteSystem.LayerSetRsiState(entity, layer, $"{RestringState}_{state}");
            return;
        }

        SpriteSystem.LayerSetRsiState(entity, layer, $"{BaseState}_{state}");
    }
}
