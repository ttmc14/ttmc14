using System.Numerics;
using Content.Shared._MC.Stun.Events;
using Content.Shared._RMC14.Pulling;
using Content.Shared._RMC14.Slow;
using Content.Shared._RMC14.Stun;
using Content.Shared._RMC14.Xenonids;
using Content.Shared.Movement.Systems;
using Content.Shared.Projectiles;
using Content.Shared.StatusEffect;
using Content.Shared.Stunnable;
using Content.Shared.Throwing;
using Robust.Shared.Physics.Systems;

namespace Content.Shared._MC.Stun;

public sealed class MCStunSystem : EntitySystem
{
    [Dependency] private readonly SharedTransformSystem _transform = null!;
    [Dependency] private readonly SharedStunSystem _stun = null!;
    [Dependency] private readonly SharedPhysicsSystem _physics = null!;
    [Dependency] private readonly ThrowingSystem _throwing = null!;
    [Dependency] private readonly StatusEffectsSystem _statusEffects = null!;
    [Dependency] private readonly MovementSpeedModifierSystem _movementSpeedModifier = null!;

    [Dependency] private readonly RMCSlowSystem _slow = null!;
    [Dependency] private readonly RMCPullingSystem _rmcPulling = null!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MCStunOnHitComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<MCStunOnHitComponent, ProjectileHitEvent>(OnHit);
    }

    private void OnMapInit(Entity<MCStunOnHitComponent> entity, ref MapInitEvent args)
    {
        if (entity.Comp.ShotFrom is not null)
            return;

        entity.Comp.ShotFrom = _transform.GetWorldPosition(entity.Owner);
        Dirty(entity);
    }

    private void OnHit(Entity<MCStunOnHitComponent> entity, ref ProjectileHitEvent args)
    {
        if (entity.Comp.ShotFrom is not { } shotFrom)
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
            Stagger(args.Target, entity.Comp.StaggerTime);
        }

        _slow.TrySlowdown(args.Target, entity.Comp.SlowdownTime);

        if (entity.Comp.Knockback != 0)
        {
            _physics.SetLinearVelocity(args.Target, Vector2.Zero);
            _physics.SetAngularVelocity(args.Target, 0f);

            _rmcPulling.TryStopPullsOn(args.Target);

            _throwing.TryThrow(args.Target, direction.Normalized() * entity.Comp.Knockback, entity.Comp.KnockbackSpeed, animated: false, playSound: false, compensateFriction: true);
        }
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

    public void Paralyze(EntityUid uid, TimeSpan duration, bool refresh = true)
    {
        if (HasComp<XenoComponent>(uid))
            duration *= 0.2f;

        _stun.TryParalyze(uid, duration, refresh: refresh);
    }

    public void Slowdown(EntityUid uid, TimeSpan duration)
    {
        _slow.TrySlowdown(uid, duration);
    }

    public void Stagger(EntityUid uid, TimeSpan duration)
    {
        if (duration == TimeSpan.Zero)
            return;

        var attemptEv = new MCStaggerAttemptEvent();
        RaiseLocalEvent(uid, ref attemptEv);

        if (attemptEv.Canceled)
            return;

        var ev = new MCStaggerEvent();
        RaiseLocalEvent(uid, ref ev);
    }

    public bool IsStun(EntityUid uid)
    {
        return HasComp<StunnedComponent>(uid);
    }

    public bool IsParalyzed(EntityUid uid)
    {
        return HasComp<StunnedComponent>(uid) || HasComp<KnockedDownComponent>(uid);
    }

    public bool TrySlowdown(Entity<StatusEffectsComponent?> entity, string key, TimeSpan time, float multiplier = 1f)
    {
        if (!Resolve(entity, ref entity.Comp, false))
            return false;

        if (!_statusEffects.TryAddStatusEffect<SlowedDownComponent>(entity, key, time, true, entity.Comp, force: true))
            return false;

        var slowed = Comp<SlowedDownComponent>(entity);
        slowed.WalkSpeedModifier = multiplier;
        slowed.SprintSpeedModifier = multiplier;

        _movementSpeedModifier.RefreshMovementSpeedModifiers(entity);
        return true;

    }
}
