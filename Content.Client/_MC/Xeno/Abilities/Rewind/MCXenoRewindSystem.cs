using Content.Shared._MC.Xeno.Abilities.Wraith.Rewind;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;
using DrawDepth = Content.Shared.DrawDepth.DrawDepth;

namespace Content.Client._MC.Xeno.Abilities.Rewind;

public sealed class MCXenoRewindSystem : MCSharedXenoRewindSystem
{
    private const int MaxAfterimages = 24;
    private const float SampleDistance = 0.3f;
    private static readonly TimeSpan ImageInterval = TimeSpan.FromMilliseconds(40);
    private static readonly Color ImageTint = new(0.62f, 0.9f, 1f, 0.65f);

    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IPrototypeManager _prototypes = default!;
    [Dependency] private readonly SpriteSystem _sprite = default!;
    [Dependency] private readonly SharedTransformSystem _transformSystem = default!;

    private ShaderInstance _mark = default!;
    private ShaderInstance _rewind = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MCXenoRewindVisualsComponent, MoveEvent>(OnMove);
        SubscribeLocalEvent<MCXenoRewindVisualsComponent, ComponentShutdown>(OnVisualsShutdown);

        _mark = CreateBlur(0.12f);
        _rewind = CreateBlur(0.45f);
    }

    private ShaderInstance CreateBlur(float strength)
    {
        var shader = _prototypes.Index<ShaderPrototype>("MCRewindBlur").InstanceUnique();
        shader.SetParameter("strength", strength);
        shader.MakeImmutable();
        return shader;
    }

    protected override void OnTargetStartup(Entity<MCXenoRewindTargetComponent> entity, ref ComponentStartup args)
    {
        base.OnTargetStartup(entity, ref args);

        if (entity.Comp.Canceled || !TryComp<SpriteComponent>(entity, out var sprite))
            return;

        var visuals = EnsureComp<MCXenoRewindVisualsComponent>(entity);
        visuals.Path.Clear();
        visuals.ImageCount = 0;
        visuals.NextImage = 0;
        var transform = Transform(entity);
        visuals.Path.Add((transform.Coordinates, transform.LocalRotation));

        if (sprite.PostShader == null || sprite.PostShader == _rewind)
            sprite.PostShader = _mark;
    }

    private void OnMove(Entity<MCXenoRewindVisualsComponent> entity, ref MoveEvent args)
    {
        if (entity.Comp.ImageCount != 0 || args.NewPosition == args.OldPosition || !_timing.IsFirstTimePredicted)
            return;

        var path = entity.Comp.Path;
        if (path.Count > 0 && _transformSystem.InRange(args.NewPosition, path[^1].Position, SampleDistance))
            return;

        path.Add((args.NewPosition, args.NewRotation));
    }

    protected override void OnTagetShutdown(Entity<MCXenoRewindTargetComponent> entity, ref ComponentShutdown args)
    {
        StartPlayback(entity);
        base.OnTagetShutdown(entity, ref args);
    }

    private void StartPlayback(Entity<MCXenoRewindTargetComponent> entity)
    {
        if (!TryComp<MCXenoRewindVisualsComponent>(entity, out var visuals))
            return;

        if (entity.Comp.Canceled || TerminatingOrDeleted(entity) || !TryComp<SpriteComponent>(entity, out var sprite))
        {
            RemCompDeferred<MCXenoRewindVisualsComponent>(entity);
            return;
        }

        visuals.ImageCount = Math.Min(visuals.Path.Count, MaxAfterimages);
        visuals.PlaybackStart = _timing.CurTime;
        if (sprite.PostShader == _mark || sprite.PostShader == null)
            sprite.PostShader = _rewind;
    }

    private void OnVisualsShutdown(Entity<MCXenoRewindVisualsComponent> entity, ref ComponentShutdown args)
    {
        if (TryComp<SpriteComponent>(entity, out var sprite) && (sprite.PostShader == _rewind || sprite.PostShader == _mark))
            sprite.PostShader = null;
    }

    public override void FrameUpdate(float frameTime)
    {
        var now = _timing.CurTime;
        var query = EntityQueryEnumerator<MCXenoRewindVisualsComponent, SpriteComponent>();
        while (query.MoveNext(out var uid, out var visuals, out var sprite))
        {
            if (visuals.ImageCount == 0)
                continue;

            var step = (int) ((now - visuals.PlaybackStart) / ImageInterval);
            var index = Math.Min(step, visuals.ImageCount - 1);
            if (index >= visuals.NextImage)
            {
                // Sample the entire route in reverse, even when rendering skips a step.
                var last = visuals.Path.Count - 1;
                var sample = last - index * last / Math.Max(1, visuals.ImageCount - 1);
                var (position, rotation) = visuals.Path[sample];
                SpawnAfterimage((uid, sprite), position, rotation);
                visuals.NextImage = index + 1;
            }

            if (step >= visuals.ImageCount)
                RemCompDeferred<MCXenoRewindVisualsComponent>(uid);
        }
    }

    private void SpawnAfterimage(Entity<SpriteComponent> target, EntityCoordinates position, Angle rotation)
    {
        if (!position.IsValid(EntityManager))
            return;

        var uid = Spawn("MCEffectWraithRewindAfterimage", position);
        _transformSystem.SetLocalRotationNoLerp(uid, rotation);

        var image = Comp<SpriteComponent>(uid);
        _sprite.CopySprite((target.Owner, target.Comp), (uid, image));
        image.PostShader = null;
        _sprite.SetDrawDepth((uid, image), (int) DrawDepth.BelowMobs);
        _sprite.SetColor((uid, image), (target.Comp.Color * ImageTint).WithAlpha(ImageTint.A));
    }
}
