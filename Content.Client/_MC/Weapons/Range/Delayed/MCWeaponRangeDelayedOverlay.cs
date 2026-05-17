using System.Numerics;
using Content.Client.UserInterface.Systems;
using Content.Shared._MC.Weapons.Range.Delayed.Components;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Shared.Enums;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;
using Robust.Shared.Utility;

namespace Content.Client._MC.Weapons.Range.Delayed;

public sealed class MCWeaponRangeDelayedOverlay : Overlay
{
    private static readonly ProtoId<ShaderPrototype> UnshadedShader = "unshaded";

    private const float StartX = 2f;
    private const float EndX = 22f;

    [Dependency] private readonly IEntityManager _entity = null!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = null!;
    [Dependency] private readonly IGameTiming _timing = null!;

    private readonly SharedTransformSystem _transform;
    private readonly ProgressColorSystem _progressColor;
    private readonly SpriteSystem _sprite;

    private readonly Texture _barTexture;
    private readonly ShaderInstance _unshadedShader;

    public override OverlaySpace Space => OverlaySpace.WorldSpaceBelowFOV;

    public MCWeaponRangeDelayedOverlay()
    {
        IoCManager.InjectDependencies(this);

        _transform = _entity.System<SharedTransformSystem>();
        _progressColor = _entity.System<ProgressColorSystem>();
        _sprite = _entity.System<SpriteSystem>();

        var sprite = new SpriteSpecifier.Rsi(new ResPath("/Textures/Interface/Misc/progress_bar.rsi"), "icon");

        _barTexture = _sprite.Frame0(sprite);
        _unshadedShader = _prototypeManager.Index(UnshadedShader).Instance();
    }

    protected override void Draw(in OverlayDrawArgs args)
    {
        var handle = args.WorldHandle;
        var rotation = args.Viewport.Eye?.Rotation ?? Angle.Zero;
        var rotationMatrix = Matrix3Helpers.CreateRotation(-rotation);
        var scaleMatrix = Matrix3Helpers.CreateScale(Vector2.One);
        var curTime = _timing.CurTime;
        var bounds = args.WorldAABB.Enlarged(5f);

        var query = _entity.EntityQueryEnumerator<
            MCWeaponRangeDelayedAlertComponent,
            TransformComponent>();

        while (query.MoveNext(out var uid, out var alert, out var xform))
        {
            var worldPos = xform.MapPosition.Position;

            if (!bounds.Contains(worldPos))
                continue;

            var total = alert.TimeEnd - alert.TimeStart;
            var elapsed = curTime - alert.TimeStart;

            if (total <= TimeSpan.Zero)
                continue;

            var progress = (float)(elapsed / total);
            progress = float.Clamp(progress, 0f, 1f);

            var worldMatrix = Matrix3Helpers.CreateTranslation(worldPos);
            var scaledWorld = Matrix3x2.Multiply(scaleMatrix, worldMatrix);
            var matrix = Matrix3x2.Multiply(rotationMatrix, scaledWorld);

            handle.SetTransform(matrix);
            handle.UseShader(_unshadedShader);

            var yOffset = 0.5f;

            if (_entity.TryGetComponent(uid, out SpriteComponent? sprite))
                yOffset = _sprite.GetLocalBounds((uid, sprite)).Height / 2f + 0.05f;

            var position = new Vector2(
                -_barTexture.Width / 2f / EyeManager.PixelsPerMeter,
                yOffset
            );

            handle.DrawTexture(_barTexture, position, Color.White);

            var color = _progressColor.GetProgressColor(progress);
            var xProgress = (EndX - StartX) * progress + StartX;

            var box = new Box2(
                new Vector2(StartX, 3f) / EyeManager.PixelsPerMeter,
                new Vector2(xProgress, 4f) / EyeManager.PixelsPerMeter
            );

            box = box.Translated(position);

            handle.DrawRect(box, color);
        }

        handle.UseShader(null);
        handle.SetTransform(Matrix3x2.Identity);
    }
}
