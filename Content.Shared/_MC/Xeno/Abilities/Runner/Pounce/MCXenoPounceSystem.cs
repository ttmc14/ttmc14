using System.Numerics;
using Content.Shared._MC.Xeno.Hive.Systems.Main;
using Content.Shared._RMC14.Actions;
using Content.Shared._RMC14.Pulling;
using Content.Shared._RMC14.Xenonids.Hive;
using Content.Shared.Damage;
using Content.Shared.DoAfter;
using Content.Shared.Mobs.Systems;
using Content.Shared.Physics;
using Content.Shared.Stunnable;
using Content.Shared.Tag;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Map;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Events;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Shared._MC.Xeno.Abilities.Runner.Pounce;

public sealed class MCXenoPounceSystem : MCXenoAbilitySystem
{
    private static readonly ProtoId<TagPrototype> AcidSprayTag = "MCAcidSpray";

    [Dependency] private readonly IGameTiming _timing = null!;

    [Dependency] private readonly SharedAudioSystem _audio = null!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = null!;
    [Dependency] private readonly MobStateSystem _mobState = null!;
    [Dependency] private readonly SharedPhysicsSystem _physics = null!;
    [Dependency] private readonly SharedTransformSystem _transform = null!;
    [Dependency] private readonly SharedStunSystem _stun = null!;
    [Dependency] private readonly DamageableSystem _damageable = null!;
    [Dependency] private readonly TagSystem _tag = null!;

    [Dependency] private readonly RMCPullingSystem _rmcPulling = null!;

    [Dependency] private readonly MCSharedXenoHiveSystem _mcXenoHive = null!;

    private EntityQuery<PhysicsComponent> _physicsQuery;

    public override void Initialize()
    {
        base.Initialize();

        _physicsQuery = GetEntityQuery<PhysicsComponent>();

        SubscribeLocalEvent<MCXenoPounceComponent, MCXenoPounceActionEvent>(OnAction);
        SubscribeLocalEvent<MCXenoPounceComponent, MCXenoPounceDoAfterEvent>(OnDoAfter);

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

        if (entity.Comp.Delay == TimeSpan.Zero)
        {
            if (UseAbility(entity, args.Action, args.Target))
                args.Handled = true;

            return;
        }

        if (!CanUseAction(entity, args.Action))
            return;

        _doAfter.TryStartDoAfter(new DoAfterArgs(EntityManager, entity, entity.Comp.Delay, new MCXenoPounceDoAfterEvent(args.Action, args.Target, EntityManager), entity)
        {
            BreakOnMove = true,
            BreakOnRest = true,
        });
    }

    private void OnDoAfter(Entity<MCXenoPounceComponent> entity, ref MCXenoPounceDoAfterEvent args)
    {
        if (args.Handled || args.Cancelled)
            return;

        args.Handled = true;

        var actionUid = GetEntity(args.ActionUid);
        var targetCoordinates = GetCoordinates(args.Coordinates);

        UseAbility(entity, actionUid, targetCoordinates);
    }

    private bool UseAbility(Entity<MCXenoPounceComponent> entity, EntityUid actionUid, EntityCoordinates targetCoordinates)
    {
        if (!TryUseAction(entity, actionUid))
            return false;

        if (!_physicsQuery.TryGetComponent(entity, out var physicsComponent))
            return false;

        if (EnsureComp<MCXenoPouncingComponent>(entity, out var pouncingComponent))
            return false;

        var origin = _transform.GetMapCoordinates(entity);
        var target = _transform.ToMapCoordinates(targetCoordinates);
        var direction = target.Position - origin.Position;

        if (direction == Vector2.Zero)
            return false;

        var length = direction.Length();
        var distance = Math.Clamp(length, 0.1f, entity.Comp.MaxDistance);

        var ev = new MCXenoPounceStartEvent(entity, origin, target, direction.Normalized(), distance);
        RaiseLocalEvent(entity, ref ev);

        direction *= distance / length;

        var impulse = direction.Normalized() * entity.Comp.Strength * physicsComponent.Mass;

        _rmcPulling.TryStopAllPullsFromAndOn(entity);

        _physics.ApplyLinearImpulse(entity, impulse, body: physicsComponent);
        _physics.SetBodyStatus(entity, physicsComponent, BodyStatus.InAir);

        var duration = _timing.CurTime + TimeSpan.FromSeconds(direction.Length() / entity.Comp.Strength);

        pouncingComponent.End = duration;
        Dirty(entity, pouncingComponent);

        ActionStartUseDelay<MCXenoPounceActionEvent>(entity, actionUid);

        return true;
    }

    private void OnHit(Entity<MCXenoPouncingComponent> entity, ref PreventCollideEvent args)
    {
        if (args.OtherFixture.CollisionLayer == (int) CollisionGroup.SlipLayer)
            return;

        if (_tag.HasTag(args.OtherEntity, AcidSprayTag))
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

        if (_mcXenoHive.FromSameHive(entity.Owner, target))
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
