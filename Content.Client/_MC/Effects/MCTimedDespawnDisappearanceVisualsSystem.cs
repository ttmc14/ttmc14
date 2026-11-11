using Content.Shared._MC.Effect;
using Robust.Client.GameObjects;
using Robust.Shared.Spawners;

namespace Content.Client._MC.Effects;

public sealed class MCTimedDespawnDisappearanceVisualsSystem : EntitySystem
{
    private const float UpdateInterval = 0.066f;

    [Dependency] private readonly SpriteSystem _spriteSystem = null!;

    private float _updateAccumulator;

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        _updateAccumulator += frameTime;
        if (_updateAccumulator < UpdateInterval)
            return;

        _updateAccumulator -= UpdateInterval;

        var query = EntityQueryEnumerator<SpriteComponent, TimedDespawnComponent, MCTimedDespawnDisappearanceVisualsComponent>();
        while (query.MoveNext(out var uid, out var sprite, out var despawn, out var visuals))
        {
            if (visuals.Lifetime <= 0f)
                continue;

            var progress = float.Clamp(despawn.Lifetime / visuals.Lifetime, 0f, 1f);
            var alpha = visuals.MinAlpha + (visuals.MaxAlpha - visuals.MinAlpha) * progress;
            if (float.Abs(sprite.Color.A - alpha) < 0.005f)
                continue;

            _spriteSystem.SetColor((uid, sprite), sprite.Color.WithAlpha(alpha));
        }
    }
}
