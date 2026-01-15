using Content.Shared._MC.Xeno.Abilities.Boiler.Bombard;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Client.Input;
using Robust.Client.Player;
using Robust.Client.UserInterface;
using Robust.Shared.Timing;
using Robust.Shared.Utility;

namespace Content.Client._MC.Xeno.Abilities.Bombard;

public sealed class MCXenoBombardOverlay : Overlay
{
    private const float Scale = 2f;

    [Dependency] private readonly IGameTiming _timing = null!;
    [Dependency] private readonly IEntityManager _entityManager = null!;
    [Dependency] private readonly IPlayerManager _player = null!;
    [Dependency] private readonly IInputManager _inputManager = null!;
    [Dependency] private readonly IEyeManager _eye = null!;

    private readonly SpriteSpecifier.Rsi _icon = new(new ResPath("/Textures/_MC/Interface/pointers.rsi"), "bombard");

    private readonly SpriteSystem _sprite;

    public MCXenoBombardOverlay()
    {
        IoCManager.InjectDependencies(this);

        _sprite = _entityManager.System<SpriteSystem>();
    }

    protected override bool BeforeDraw(in OverlayDrawArgs args)
    {
        return ValidEntity(_player.LocalEntity);
    }

    protected override void Draw(in OverlayDrawArgs args)
    {
        if (_player.LocalEntity is null)
            return;

        var mouseScreenPosition = _inputManager.MouseScreenPosition;
        var mousePosMap = _eye.PixelToMap(mouseScreenPosition);

        if (mousePosMap.MapId != args.MapId)
            return;

        var mousePos = mouseScreenPosition.Position;
        var uiScale = (args.ViewportControl as Control)?.UIScale ?? 1f;
        var limitedScale = uiScale > 1.25f ? 1.25f : uiScale;

        var component = _entityManager.GetComponent<Shared._MC.Xeno.Abilities.Boiler.Bombard.Components.MCXenoBombardComponent>(_player.LocalEntity.Value);
        if (!component.Digging)
            return;

        var texture = _sprite.GetFrame(_icon, _timing.CurTime);
        var size = texture.Size * limitedScale * Scale;
        var rect = UIBox2.FromDimensions(mousePos - size * 0.5f, size);
        args.ScreenHandle.DrawTextureRect(texture, rect);
    }

    private bool ValidEntity(EntityUid? uid)
    {
        return _entityManager.HasComponent<Shared._MC.Xeno.Abilities.Boiler.Bombard.Components.MCXenoBombardComponent>(uid);
    }
}
