using Content.Shared._MC.Xeno.Abilities.Evasion;
using Content.Shared._MC.Xeno.Abilities.ToxicStacks;
using Content.Shared._RMC14.Xenonids;
using Robust.Client.GameObjects;
using Robust.Client.Player;

namespace Content.Client._MC.Xeno.Abilities.ToxicStacks;

public sealed class MCXenoEvasionVisualizerSystem : VisualizerSystem<MCXenoToxicStacksComponent>
{
    [Dependency] private readonly IPlayerManager _player = default!;

    protected override void OnAppearanceChange(EntityUid uid, MCXenoToxicStacksComponent component, ref AppearanceChangeEvent args)
    {
        base.OnAppearanceChange(uid, component, ref args);

        if (args.Sprite is null)
            return;

        var sprite = new Entity<SpriteComponent?>(uid, args.Sprite);
        if (!SpriteSystem.LayerMapTryGet(sprite, MCXenoToxicStacksLayer.Base, out var layer, false) ||
            !SpriteSystem.LayerMapTryGet(sprite, MCXenoToxicStacksLayer.Icon, out var iconLayer, false))
            return;

        if (_player.LocalEntity is null || !HasComp<XenoComponent>(_player.LocalEntity))
        {
            SpriteSystem.LayerSetVisible(sprite, layer, false);
            SpriteSystem.LayerSetVisible(sprite, iconLayer, false);
            return;
        }

        if (!AppearanceSystem.TryGetData<int>(uid, MCXenoToxicStacksVisuals.Visuals, out var value, args.Component))
            return;

        var visible = value > 0;
        SpriteSystem.LayerSetVisible(sprite, layer, visible);
        SpriteSystem.LayerSetVisible(sprite, iconLayer, visible);
        SpriteSystem.LayerSetRsiState(sprite, layer, $"intoxicated_amount{value}");
    }
}
