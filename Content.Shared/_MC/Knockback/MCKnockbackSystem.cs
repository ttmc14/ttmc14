using System.Numerics;
using Content.Shared._RMC14.Pulling;
using Content.Shared.Throwing;

namespace Content.Shared._MC.Knockback;

public sealed class MCKnockbackSystem : EntitySystem
{
    [Dependency] private readonly RMCPullingSystem _rmcPulling = default!;
    [Dependency] private readonly ThrowingSystem _throwing = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    public void Knockback(EntityUid uid, Vector2 direction, float distance, float speed, bool compensateFriction = false, bool animated = true)
    {
        if (Transform(uid).Anchored)
            return;

        _rmcPulling.TryStopAllPullsFromAndOn(uid);
        _throwing.TryThrow(uid, direction.Normalized() * distance, speed);
    }

    public void KnockbackFrom(EntityUid uid, EntityUid from, float distance, float speed)
    {
        var origin = _transform.GetMapCoordinates(from);
        var direction = _transform.GetMapCoordinates(uid).Position - origin.Position;
        Knockback(uid, direction, distance, speed);
    }
}
