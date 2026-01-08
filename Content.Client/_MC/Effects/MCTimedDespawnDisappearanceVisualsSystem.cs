using Content.Shared._MC.Effect;
using Robust.Client.GameObjects;
using Robust.Shared.Spawners;

namespace Content.Client._MC.Effects;

public sealed class MCTimedDespawnDisappearanceVisualsSystem : EntitySystem
{
    [Dependency] private readonly SpriteSystem _spriteSystem = null!;

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<SpriteComponent, TimedDespawnComponent, MCTimedDespawnDisappearanceVisualsComponent>();
        while (query.MoveNext(out var uid, out var spriteComponent, out var despawnComponent, out var visualsComponent))
        {
            if (visualsComponent.Lifetime <= 0f)
                continue;

            var progress = MathHelper.Clamp(
                despawnComponent.Lifetime / visualsComponent.Lifetime,
                0f,
                1f
            );

            var alpha = MathHelper.Lerp(
                visualsComponent.MinAlpha,
                visualsComponent.MaxAlpha,
                progress);

            var color = spriteComponent.Color;
            var newColor = color.WithAlpha(alpha);

            if (color.Equals(newColor))
                continue;

            _spriteSystem.SetColor((uid, spriteComponent), newColor);
        }
    }
}
