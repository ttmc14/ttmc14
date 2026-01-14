using System.Numerics;
using Content.Client._RMC14.NightVision;
using Content.Client.Chemistry.Containers.EntitySystems;
using Content.Shared._MC.Xeno.Hud;
using Content.Shared._RMC14.Mobs;
using Content.Shared._RMC14.Xenonids;
using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.Components.SolutionManager;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.FixedPoint;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Shared.Enums;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;
using Robust.Shared.Utility;

namespace Content.Client._MC.Xeno.Hud;

public sealed class MCXenoHudToxinsOverlay : Overlay
{
    [Dependency] private readonly IGameTiming _timing = null!;
    [Dependency] private readonly IEntityManager _entityManager = null!;
    [Dependency] private readonly IPrototypeManager _prototype = null!;
    [Dependency] private readonly IPlayerManager _player = null!;

    private readonly SpriteSystem _sprite;
    private readonly TransformSystem _transform;
    private readonly SolutionContainerSystem _solutionContainer;

    private readonly EntityQuery<TransformComponent> _xformQuery;

    private readonly ResPath _toxinPath = new("/Textures/_MC/Interface/Hud/toxins.rsi");

    private readonly ShaderInstance _shader;

    public override OverlaySpace Space => OverlayManager.HasOverlay<NightVisionOverlay>()
        ? OverlaySpace.WorldSpace
        : OverlaySpace.WorldSpaceBelowFOV;

    public MCXenoHudToxinsOverlay()
    {
        IoCManager.InjectDependencies(this);

        _sprite = _entityManager.System<SpriteSystem>();
        _transform = _entityManager.System<TransformSystem>();
        _solutionContainer = _entityManager.System<SolutionContainerSystem>();

        _xformQuery = _entityManager.GetEntityQuery<TransformComponent>();

        _shader = _prototype.Index<ShaderPrototype>("unshaded").Instance();

        ZIndex = 2;
    }

    protected override void Draw(in OverlayDrawArgs args)
    {
        var handle = args.WorldHandle;

        handle.UseShader(_shader);

        DrawToxinsIcons(in args);

        handle.UseShader(null);
        handle.SetTransform(Matrix3x2.Identity);
    }

    private void DrawToxinsIcons(in OverlayDrawArgs args)
    {
        if (!ValidEntity(_player.LocalEntity))
            return;

        var handle = args.WorldHandle;

        var query = _entityManager.AllEntityQueryEnumerator<MCXenoHudToxinsComponent, SolutionContainerManagerComponent, SpriteComponent, TransformComponent>();
        while (query.MoveNext(out var uid, out var component, out var solutionContainer,  out var sprite, out var xform))
        {
            if (xform.MapID != args.MapId)
                continue;

            if (!_solutionContainer.TryGetSolution((uid, solutionContainer), component.Solution, out _, out var solution))
                continue;

            var bounds = _sprite.GetLocalBounds((uid, sprite));
            var worldPosition = _transform.GetWorldPosition(xform, _xformQuery);

            if (!bounds.Translated(worldPosition).Intersects(args.WorldAABB))
                continue;

            var worldPos = _transform.GetWorldPosition(xform, _xformQuery);
            handle.SetTransform(Matrix3x2.CreateTranslation(worldPos));

            foreach (var (reagentId, state) in component.Reagents)
            {
                if (!solution.TryGetReagentQuantity(new ReagentId(reagentId, null), out var quantity))
                    continue;

                var reagentState = quantity > component.ReagentHighQuantity ? $"{state}{component.ReagentHighPostfix}" : state;
                DrawToxin(reagentState, bounds, sprite, handle);
            }

            handle.SetTransform(Matrix3x2.Identity);
        }
    }

    private void DrawToxin(string state, Box2 bounds, SpriteComponent spriteComponent, DrawingHandleWorld worldHandle)
    {
        var texture = _sprite.GetFrame(new SpriteSpecifier.Rsi(_toxinPath, state), _timing.CurTime);

        var size = texture.Size / EyeManager.PixelsPerMeter;
        var position = new Vector2(
            -size.X / 2f,
            -size.Y / 2f
        );

        worldHandle.DrawTexture(texture, position);
    }

    private bool ValidEntity(EntityUid? uid)
    {
        return _entityManager.HasComponent<XenoComponent>(uid) ||
               _entityManager.HasComponent<CMGhostXenoHudComponent>(uid);
    }
}
