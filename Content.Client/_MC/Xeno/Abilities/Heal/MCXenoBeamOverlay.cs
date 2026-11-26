using System.Numerics;
using Content.Shared._MC.Xeno.Abilities.Gorger.PsychicLink.Components;
using Content.Shared._MC.Xeno.Abilities.Gorger.Transfusion.Components;
using Content.Shared._MC.Xeno.Abilities.Heal.HealingInfusion.Components;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Shared.Enums;
using Robust.Shared.Timing;
using Robust.Shared.Utility;

namespace Content.Client._MC.Xeno.Abilities.Heal;

/// <summary>
/// Draws animated psychic and healing beams between the current world positions of their endpoints.
/// </summary>
public sealed class MCXenoBeamOverlay : Overlay
{
    private static readonly ResPath BeamPath = new("/Textures/_MC/Effects/Beams/blood.rsi");
    private static readonly SpriteSpecifier.Rsi Link = new(BeamPath, "blood_light");
    private static readonly SpriteSpecifier.Rsi Transfusion = new(BeamPath, "lichbeam");

    private static readonly SpriteSpecifier.Rsi Infusion = new(new ResPath("/Textures/_RMC14/Effects/beam.rsi"), "medbeam");

    [Dependency] private readonly IEntityManager _entities = null!;
    [Dependency] private readonly IGameTiming _timing = null!;

    private readonly SpriteSystem _sprites;
    private readonly SharedTransformSystem _transform;

    public override OverlaySpace Space => OverlaySpace.WorldSpaceBelowFOV;

    public MCXenoBeamOverlay()
    {
        IoCManager.InjectDependencies(this);
        _sprites = _entities.System<SpriteSystem>();
        _transform = _entities.System<SharedTransformSystem>();
    }

    protected override void Draw(in OverlayDrawArgs args)
    {
        var links = _entities.EntityQueryEnumerator<MCXenoPsychicLinkComponent, TransformComponent>();
        while (links.MoveNext(out var link, out var origin))
        {
            DrawBeam(args, origin, link.Target, link.Range, Link, Color.White);
        }

        var infusions = _entities.EntityQueryEnumerator<MCXenoHealingInfusionBeamComponent, TransformComponent>();
        while (infusions.MoveNext(out var infusion, out var origin))
        {
            DrawBeam(args, origin, infusion.Recipient, infusion.Range, Infusion, Color.White);
        }

        var transfusions = _entities.EntityQueryEnumerator<MCXenoTransfusionComponent, TransformComponent>();
        while (transfusions.MoveNext(out var channel, out var origin))
        {
            // Lichbeam scrolls towards the top of each frame.
            DrawBeam(args, origin, channel.Recipient, channel.Range, Transfusion, Color.White.WithAlpha(0.5f), Angle.FromDegrees(180));
        }
    }

    private void DrawBeam(in OverlayDrawArgs args,
        TransformComponent origin,
        EntityUid? target,
        float range,
        SpriteSpecifier.Rsi icon,
        Color color,
        Angle spriteRotation = default)
    {
        if (origin.MapID != args.MapId || !_entities.TryGetComponent<TransformComponent>(target, out var end)
            || end.MapID != args.MapId)
            return;

        var start = _transform.GetWorldPosition(origin);
        var finish = _transform.GetWorldPosition(end);
        if (!Box2.FromTwoPoints(start, finish).Enlarged(0.5f).Intersects(args.WorldAABB))
            return;

        var delta = finish - start;
        var length = delta.Length();
        if (length <= 0 || length > range)
            return;

        var direction = delta / length;
        var angle = direction.ToWorldAngle() + spriteRotation;
        var texture = _sprites.GetFrame(icon, _timing.CurTime);
        var segments = (int) MathF.Ceiling(length);
        var step = length / segments;
        for (var i = 0; i < segments; i++)
        {
            var center = start + direction * ((i + 0.5f) * step);
            var box = Box2.CenteredAround(center, new Vector2(1f, step));
            args.WorldHandle.DrawTextureRect(texture, new Box2Rotated(box, angle, center), color);
        }
    }
}
