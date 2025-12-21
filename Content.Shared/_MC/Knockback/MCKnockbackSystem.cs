using System.Numerics;
using Content.Shared._RMC14.Pulling;
using Content.Shared.Throwing;

namespace Content.Shared._MC.Knockback;

public sealed class MCKnockbackSystem : EntitySystem
{
    [Dependency] private readonly RMCPullingSystem _rmcPulling = null!;
    [Dependency] private readonly ThrowingSystem _throwing = null!;
    [Dependency] private readonly SharedTransformSystem _transform = null!;

    public void Charge(EntityUid uid)
    {

    }

    public void Knockback(EntityUid uid, Vector2 direction, MCKnockbackEntry entry, bool compensateFriction = true, bool animated = true)
    {
        Knockback(uid, direction, entry.Distance, entry.Speed, compensateFriction, animated);
    }

    public void Knockback(EntityUid uid, Vector2 direction, float distance, float speed, bool compensateFriction = true, bool animated = true)
    {
        if (Transform(uid).Anchored)
            return;

        if (direction == Vector2.Zero)
            return;

        _rmcPulling.TryStopAllPullsFromAndOn(uid);
        _throwing.TryThrow(uid, direction.Normalized() * distance, speed, compensateFriction: compensateFriction, animated: animated);
    }

    public void KnockbackFrom(EntityUid uid, EntityUid from, MCKnockbackEntry entry)
    {
        KnockbackFrom(uid, from, entry.Distance, entry.Speed);
    }

    public void KnockbackFrom(EntityUid uid, EntityUid from, float distance, float speed)
    {
        var direction = _transform.GetWorldPosition(uid) - _transform.GetWorldPosition(from);
        Knockback(uid, direction, distance, speed);
    }
}
