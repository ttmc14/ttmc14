using Content.Shared._MC.Xeno.Abilities.Gorger.Feast.Components;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Shared.Enums;
using Robust.Shared.Timing;
using Robust.Shared.Utility;

namespace Content.Client._MC.Xeno.Abilities.Gorger.Feast;

/// <summary>
/// Displays the animated bloodlust vignette in the feasting player's viewport.
/// </summary>
public sealed class MCXenoFeastOverlay : Overlay
{
    private static readonly SpriteSpecifier.Rsi Bloodlust = new(new ResPath("/Textures/_MC/Effects/Xeno/bloodlust.rsi"), "bloodlust");

    [Dependency] private readonly IEntityManager _entities = null!;
    [Dependency] private readonly IPlayerManager _player = null!;
    [Dependency] private readonly IGameTiming _timing = null!;

    private readonly SpriteSystem _sprites;

    public override OverlaySpace Space => OverlaySpace.ScreenSpace;

    public MCXenoFeastOverlay()
    {
        IoCManager.InjectDependencies(this);
        _sprites = _entities.System<SpriteSystem>();
    }

    protected override bool BeforeDraw(in OverlayDrawArgs args) =>
        _entities.HasComponent<MCXenoFeastActiveComponent>(_player.LocalEntity)
        && _entities.TryGetComponent<EyeComponent>(_player.LocalEntity, out var eye)
        && args.Viewport.Eye == eye.Eye;

    protected override void Draw(in OverlayDrawArgs args) =>
        args.ScreenHandle.DrawTextureRect(_sprites.GetFrame(Bloodlust, _timing.CurTime), args.ViewportBounds);
}
