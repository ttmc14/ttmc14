using System.Numerics;
using Content.Shared._MC.Stun.Events;
using Content.Shared._RMC14.Pulling;
using Content.Shared._RMC14.Slow;
using Content.Shared._RMC14.Stun;
using Content.Shared._RMC14.Xenonids;
using Content.Shared.Projectiles;
using Content.Shared.Stunnable;
using Content.Shared.Throwing;
using Robust.Shared.Physics.Systems;

namespace Content.Shared._MC.Stun;

public sealed class MCStunSystem : EntitySystem
{
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly SharedStunSystem _stun = default!;
    [Dependency] private readonly RMCSlowSystem _slow = default!;
    [Dependency] private readonly SharedPhysicsSystem _physics = default!;
    [Dependency] private readonly RMCPullingSystem _rmcPulling = default!;
    [Dependency] private readonly ThrowingSystem _throwing = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MCStunOnHitComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<MCStunOnHitComponent, ProjectileHitEvent>(OnHit);
    }

    private void OnMapInit(Entity<MCStunOnHitComponent> entity, ref MapInitEvent args)
    {
        entity.Comp.ShotFrom = _transform.GetWorldPosition(entity.Owner);
        Dirty(entity);
    }

    private void OnHit(Entity<MCStunOnHitComponent> entity, ref ProjectileHitEvent args)
    {
        if (entity.Comp.ShotFrom is not {} shotFrom)
            return;

        var direction = _transform.GetWorldPosition(args.Target) - shotFrom;
        var distance = direction.Length();
        if (distance > entity.Comp.MaxDistance)
            return;

        if (TryComp<RMCSizeComponent>(args.Target, out var sizeComponent) && sizeComponent.Size == RMCSizes.Big)
            return;

        if (!IsParalyzed(args.Target))
        {
            Stun(args.Target, entity.Comp.StunTime);
            Paralyze(args.Target, entity.Comp.ParalyzeTime);
        }

        if (entity.Comp.Knockback == 0)
            return;

        _slow.TrySlowdown(args.Target, entity.Comp.SlowdownTime);

        _physics.SetLinearVelocity(args.Target, Vector2.Zero);
        _physics.SetAngularVelocity(args.Target, 0f);

        _rmcPulling.TryStopPullsOn(args.Target);

        _throwing.TryThrow(args.Target, direction.Normalized() * entity.Comp.Knockback, entity.Comp.KnockbackSpeed, animated: false, playSound: false, compensateFriction: true);
    }

    public void Stun(EntityUid uid, TimeSpan duration)
    {
        var ev = new MCStunAttemptEvent();
        RaiseLocalEvent(uid, ref ev);

        if (ev.Canceled)
            return;

        if (HasComp<XenoComponent>(uid))
            duration *= 0.5f;

        _stun.TryStun(uid, duration, refresh: true);
    }

    public void Paralyze(EntityUid uid, TimeSpan duration)
    {
        if (HasComp<XenoComponent>(uid))
            duration *= 0.2f;

        _stun.TryParalyze(uid, duration, refresh: true);
    }

    public void Slowdown(EntityUid uid, TimeSpan duration)
    {
        _slow.TrySlowdown(uid, duration);
    }

    public void Stagger(EntityUid uid, TimeSpan duration)
    {
        var ev = new MCStaggerAttemptEvent();
        RaiseLocalEvent(uid, ref ev);

        if (ev.Canceled)
            return;
    }

    public bool IsStun(EntityUid uid)
    {
        return HasComp<StunnedComponent>(uid);
    }

    public bool IsParalyzed(EntityUid uid)
    {
        return HasComp<StunnedComponent>(uid) || HasComp<KnockedDownComponent>(uid);
    }
}
