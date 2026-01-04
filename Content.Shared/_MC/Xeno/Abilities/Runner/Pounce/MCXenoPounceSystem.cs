using System.Numerics;
using Content.Shared._RMC14.Actions;
using Content.Shared._RMC14.Pulling;
using Content.Shared._RMC14.Xenonids.Hive;
using Content.Shared.Damage;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Physics;
using Content.Shared.Stunnable;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Events;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Timing;

namespace Content.Shared._MC.Xeno.Abilities.Pounce;

public sealed class MCXenoPounceSystem : MCXenoAbilitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;

    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly SharedPhysicsSystem _physics = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly SharedStunSystem _stun = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;

    [Dependency] private readonly SharedXenoHiveSystem _rmcXenoHive = default!;
    [Dependency] private readonly RMCPullingSystem _rmcPulling = default!;
    [Dependency] private readonly SharedRMCActionsSystem _rmcActions = default!;

    private EntityQuery<PhysicsComponent> _physicsQuery;

    public override void Initialize()
    {
        base.Initialize();

        _physicsQuery = GetEntityQuery<PhysicsComponent>();

        SubscribeLocalEvent<MCXenoPounceComponent, MCXenoPounceActionEvent>(OnAction);
        SubscribeLocalEvent<MCXenoPouncingComponent, PreventCollideEvent>(OnHit);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<MCXenoPouncingComponent>();
        while (query.MoveNext(out var entityUid, out var pouncingComponent))
        {
            if (_timing.CurTime < pouncingComponent.End)
                continue;

            Stop(entityUid);
        }
    }

    private void OnAction(Entity<MCXenoPounceComponent> entity, ref MCXenoPounceActionEvent args)
    {
        if (args.Handled)
            return;

        if (!_rmcActions.TryUseAction(entity, args.Action, entity))
            return;

        args.Handled = true;

        if (!_physicsQuery.TryGetComponent(entity, out var physicsComponent))
            return;

        if (EnsureComp<MCXenoPouncingComponent>(entity, out var pouncingComponent))
            return;

        var origin = _transform.GetMapCoordinates(entity);
        var target = _transform.ToMapCoordinates(args.Target);
        var direction = target.Position - origin.Position;

        if (direction == Vector2.Zero)
            return;

        var length = direction.Length();
        var distance = Math.Clamp(length, 0.1f, entity.Comp.MaxDistance);

        direction *= distance / length;

        var impulse = direction.Normalized() * entity.Comp.Strength * physicsComponent.Mass;

        _rmcPulling.TryStopAllPullsFromAndOn(entity);

        _physics.ApplyLinearImpulse(entity, impulse, body: physicsComponent);
        _physics.SetBodyStatus(entity, physicsComponent, BodyStatus.InAir);

        var duration = _timing.CurTime + TimeSpan.FromSeconds(direction.Length() / entity.Comp.Strength);

        pouncingComponent.End = duration;
        Dirty(entity, pouncingComponent);
    }

    private void OnHit(Entity<MCXenoPouncingComponent> entity, ref PreventCollideEvent args)
    {
        if (args.OtherFixture.CollisionLayer == (int) CollisionGroup.SlipLayer)
            return;

        if (entity.Comp.Hit.Contains(args.OtherEntity))
        {
            args.Cancelled = true;
            return;
        }

        entity.Comp.Hit.Add(args.OtherEntity);
        Hit(entity, args.OtherEntity);

        if (!IsMob(args.OtherEntity))
            return;

        args.Cancelled = true;
    }

    private void Hit(Entity<MCXenoPouncingComponent> entity, EntityUid target)
    {
        if (!IsMob(target))
        {
            Stop(entity);
            return;
        }

        if (_mobState.IsDead(target))
            return;

        if (_rmcXenoHive.FromSameHive(entity.Owner, target))
        {
            Stop(entity);
            return;
        }

        if (!TryComp<MCXenoPounceComponent>(entity, out var pounceComponent))
            return;

        if (pounceComponent.StopOnHit)
            Stop(entity);

        // TODO: work with shields

        _stun.TrySlowdown(entity, pounceComponent.HitSelfParalyzeTime, true, 0f, 0f);
        _stun.TryParalyze(target, pounceComponent.HitKnockdownTime, true);

        if (pounceComponent.HitDamage is { } damage)
        {
            _damageable.TryChangeDamage(target, damage, origin: entity, tool: entity);
            RaiseEffect(entity, target);
        }

        var first = entity.Comp.Hit.Count == 1;

        if (pounceComponent.HitSound is not null && first)
            _audio.PlayPredicted(pounceComponent.HitSound, entity, entity);

        var ev = new MCXenoPounceHitEvent(target, first);
        RaiseLocalEvent(entity, ref ev);
    }

    private void Stop(EntityUid entityUid)
    {
        if (!_physicsQuery.TryGetComponent(entityUid, out var physics))
            return;

        _physics.SetLinearVelocity(entityUid, Vector2.Zero, body: physics);
        _physics.SetBodyStatus(entityUid, physics, BodyStatus.OnGround);

        RemCompDeferred<MCXenoPouncingComponent>(entityUid);
    }
}
