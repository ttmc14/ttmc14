using Content.Shared._MC.Xeno.Abilities.Warrior.Momentum;
using Content.Shared._RMC14.Xenonids;
using Robust.Client.GameObjects;
using Robust.Client.Player;

namespace Content.Client._MC.Xeno.Abilities.Momentum;

public sealed class MCXenoMomentumVisualizerSystem : VisualizerSystem<MCXenoMomentumComponent>
{
    [Dependency] private readonly IPlayerManager _player = null!;

    protected override void OnAppearanceChange(EntityUid uid, MCXenoMomentumComponent component, ref AppearanceChangeEvent args)
    {
        if (_player.LocalEntity is null || !HasComp<XenoComponent>(_player.LocalEntity))
            return;

        if (args.Sprite is null)
            return;

        if (!AppearanceSystem.TryGetData<bool>(uid, MCXenoMomentumLayer.Base, out var value, args.Component))
            return;

        if (!SpriteSystem.LayerMapTryGet((uid, args.Sprite), MCXenoMomentumLayer.Base, out var layer, false))
            return;

        SpriteSystem.LayerSetVisible((uid, args.Sprite), layer, value);

        if (!AppearanceSystem.TryGetData<int>(uid, MCXenoMomentumVisuals.Visuals, out var count, args.Component))
            return;

        SpriteSystem.LayerSetRsiState((uid, args.Sprite), layer, $"stack_{count}");
    }
}
