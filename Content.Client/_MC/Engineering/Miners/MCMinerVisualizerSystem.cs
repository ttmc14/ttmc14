using Content.Shared._MC.Engineering.Miners.Components;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;

namespace Content.Client._MC.Engineering.Miners;

public sealed class MCMinerVisualizerSystem : MCVisualizerSystem<MCMinerVisualizerComponent>
{
    protected override void OnAppearanceChange(Entity<MCMinerVisualizerComponent> entity, ref AppearanceChangeEvent args)
    {
        if (!AppearanceSystem.TryGetData<MCMinerState>(entity, MCMinerLayers.State, out var state, args.Component))
            return;

        if (!AppearanceSystem.TryGetData<string>(entity, MCMinerLayers.Module, out var module, args.Component))
            return;

        var postfix = entity.Comp.StatesState[state];
        var resolved = new RSI.StateId($"{module}{postfix}");

        SpriteSystem.LayerSetRsi(entity.Owner, "base", entity.Comp.Sprite, resolved);
    }
}
