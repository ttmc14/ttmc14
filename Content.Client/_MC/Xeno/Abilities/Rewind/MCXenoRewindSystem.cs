using Content.Shared._MC.Effect;
using Content.Shared._MC.Xeno.Abilities.Wraith.Rewind;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Robust.Shared.Spawners;
using Robust.Shared.Timing;
using DrawDepth = Content.Shared.DrawDepth.DrawDepth;

namespace Content.Client._MC.Xeno.Abilities.Rewind;

public sealed class MCXenoRewindSystem : MCSharedXenoRewindSystem
{
    private const int MaxAfterimages = 10;
    private const float AfterimageAlpha = 0.65f;

    [Dependency] private readonly IPrototypeManager _prototypes = default!;
    [Dependency] private readonly SpriteSystem _sprites = default!;

    private readonly Dictionary<EntityUid, List<(EntityCoordinates Position, Angle Rotation)>> _paths = new();
    private ShaderInstance _mark = default!;
    private ShaderInstance _rewind = default!;

    public override void Initialize()
    {
        base.Initialize();
        var shader = _prototypes.Index<ShaderPrototype>("MCRewindBlur");
        _mark = shader.InstanceUnique();
        _mark.SetParameter("strength", 0.12f);
        _mark.MakeImmutable();
        _rewind = shader.InstanceUnique();
        _rewind.SetParameter("strength", 0.45f);
        _rewind.MakeImmutable();
    }

    public override void FrameUpdate(float frameTime)
    {
        var query = EntityQueryEnumerator<MCXenoRewindTargetComponent, SpriteComponent, TransformComponent>();
        while (query.MoveNext(out var uid, out _, out var sprite, out var xform))
        {
            sprite.PostShader ??= _mark;
            if (!_paths.TryGetValue(uid, out var path))
                _paths[uid] = path = new();

            var position = xform.Coordinates;
            if (path.Count == 0 || !position.InRange(EntityManager, path[^1].Position, 0.75f))
                path.Add((position, xform.LocalRotation));
        }
    }

    protected override void OnTagetShutdown(Entity<MCXenoRewindTargetComponent> entity, ref ComponentShutdown args)
    {
        if (_paths.Remove(entity.Owner, out var path) && TryComp<SpriteComponent>(entity, out var sprite))
        {
            if (sprite.PostShader == _mark)
                sprite.PostShader = null;

            if (!entity.Comp.Canceled && !TerminatingOrDeleted(entity))
            {
                const int stepMilliseconds = 40;
                var count = Math.Min(path.Count, MaxAfterimages);
                if (sprite.PostShader == null)
                {
                    sprite.PostShader = _rewind;
                    Timer.Spawn(count * stepMilliseconds, () =>
                    {
                        if (sprite.PostShader == _rewind)
                            sprite.PostShader = null;
                    });
                }

                for (var i = 0; i < count; i++)
                {
                    // Spread the silhouettes over the whole route, including both endpoints.
                    var index = path.Count - 1 - i * (path.Count - 1) / Math.Max(1, count - 1);
                    var (position, rotation) = path[index];
                    Timer.Spawn(i * stepMilliseconds, () => SpawnAfterimage(entity.Owner, position, rotation));
                }
            }
        }

        base.OnTagetShutdown(entity, ref args);
    }

    private void SpawnAfterimage(EntityUid target, EntityCoordinates position, Angle rotation)
    {
        if (!position.IsValid(EntityManager) || !TryComp<SpriteComponent>(target, out var source))
            return;

        var image = Spawn("clientsideclone", position);
        Transform(image).LocalRotation = rotation;
        var sprite = Comp<SpriteComponent>(image);
        _sprites.CopySprite((target, source), (image, sprite));
        sprite.PostShader = null;
        _sprites.SetDrawDepth((image, sprite), (int) DrawDepth.BelowMobs);
        _sprites.SetColor((image, sprite), (source.Color * new Color(0.62f, 0.9f, 1f)).WithAlpha(AfterimageAlpha));
        var fade = EnsureComp<MCTimedDespawnDisappearanceVisualsComponent>(image);
        fade.MaxAlpha = AfterimageAlpha;
        fade.Lifetime = EnsureComp<TimedDespawnComponent>(image).Lifetime = 0.5f;
    }
}
