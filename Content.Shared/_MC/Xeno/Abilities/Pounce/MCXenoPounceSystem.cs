using System.Numerics;
using Content.Shared._MC.Knockback;
using Content.Shared._RMC14.Actions;
using Content.Shared._RMC14.Pulling;
using Content.Shared._RMC14.Xenonids.Hive;
using Content.Shared.Damage;
using Content.Shared.Mobs.Components;
using Content.Shared.Stunnable;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Events;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Timing;

namespace Content.Shared._MC.Xeno.Abilities.Pounce;

public sealed class MCXenoPounceSystem : EntitySystem
{
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly RMCActionsSystem _rmcActions = default!;
    [Dependency] private readonly RMCPullingSystem _rmcPulling = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedPhysicsSystem _physics = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly SharedXenoHiveSystem _xenoHive = default!;
    [Dependency] private readonly SharedStunSystem _stun = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;

    private EntityQuery<PhysicsComponent> _physicsQuery;

    public override void Initialize()
    {
        base.Initialize();

        _physicsQuery = GetEntityQuery<PhysicsComponent>();

        SubscribeLocalEvent<MCXenoPounceComponent, MCXenoPounceActionEvent>(OnAction);
        SubscribeLocalEvent<MCXenoPouncingComponent, StartCollideEvent>(OnHit);
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

    private void OnHit(Entity<MCXenoPouncingComponent> entity, ref StartCollideEvent args)
    {
        if (_xenoHive.FromSameHive(entity.Owner, args.OtherEntity))
            return;

        if (!TryComp<MCXenoPounceComponent>(entity, out var pounceComponent))
            return;

        if (!HasComp<MobStateComponent>(args.OtherEntity))
            return;

        Stop(entity);

        // TODO: work with shields

        _stun.TrySlowdown(entity, pounceComponent.HitSelfParalyzeTime, true, 0f, 0f);

        _damageable.TryChangeDamage(args.OtherEntity, pounceComponent.HitDamage, origin: entity, tool: entity);
        _stun.TryParalyze(args.OtherEntity, pounceComponent.HitKnockdownTime, true);

        if (pounceComponent.HitSound is not null)
            _audio.PlayPredicted(pounceComponent.HitSound, entity, entity);
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
