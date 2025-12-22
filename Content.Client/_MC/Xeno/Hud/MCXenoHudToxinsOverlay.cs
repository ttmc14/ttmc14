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
        var eyeRotation = args.Viewport.Eye?.Rotation ?? default;
        var handle = args.WorldHandle;

        handle.UseShader(_shader);

        var scaleMatrix = Matrix3x2.CreateScale(new Vector2(1, 1));
        var rotationMatrix = Matrix3Helpers.CreateRotation(-eyeRotation);

        DrawToxinsIcons(in args, scaleMatrix, rotationMatrix);

        handle.UseShader(null);
        handle.SetTransform(Matrix3x2.Identity);
    }

    private void DrawToxinsIcons(in OverlayDrawArgs args, Matrix3x2 scaleMatrix, Matrix3x2 rotationMatrix)
    {
        if (!_entityManager.HasComponent<XenoComponent>(_player.LocalEntity) && !_entityManager.HasComponent<CMGhostXenoHudComponent>(_player.LocalEntity))
            return;

        var handle = args.WorldHandle;
        var query = _entityManager
            .AllEntityQueryEnumerator<MCXenoHudToxinsComponent, SolutionContainerManagerComponent, SpriteComponent, TransformComponent>();

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

            var worldMatrix = Matrix3x2.CreateTranslation(worldPosition);
            var worldScaled = Matrix3x2.Multiply(scaleMatrix, worldMatrix);

            handle.SetTransform(Matrix3x2.Multiply(rotationMatrix, worldScaled));

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
        var position = new Vector2(
            (bounds.Height + spriteComponent.Offset.Y) / 2f - (float) texture.Height / EyeManager.PixelsPerMeter * bounds.Height,
            (bounds.Width + spriteComponent.Offset.X) / 2f - (float) texture.Width / EyeManager.PixelsPerMeter * bounds.Width
        );

        worldHandle.DrawTexture(texture, position);
    }
}
