using Content.Shared._MC.Knockback;
using Content.Shared._MC.Xeno.Abilities.Agility;
using Content.Shared._RMC14.Actions;
using Content.Shared._RMC14.Damage.ObstacleSlamming;
using Content.Shared._RMC14.Marines;
using Content.Shared._RMC14.Pulling;
using Content.Shared._RMC14.Xenonids;
using Content.Shared._RMC14.Xenonids.Hive;
using Content.Shared.Interaction;
using Content.Shared.Mobs.Systems;
using Content.Shared.Movement.Pulling.Events;
using Content.Shared.Movement.Pulling.Systems;
using Content.Shared.StatusEffect;
using Content.Shared.Stunnable;
using Content.Shared.Throwing;
using Robust.Shared.Network;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Timing;

namespace Content.Shared._MC.Xeno.Abilities.Lunge;

public sealed class MCXenoLungeSystem : EntitySystem
{
    [Dependency] private readonly SharedInteractionSystem _interaction = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly ThrownItemSystem _thrownItem = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly SharedPhysicsSystem _physics = default!;
    [Dependency] private readonly PullingSystem _pulling = default!;
    [Dependency] private readonly StatusEffectsSystem _statusEffects = default!;
    [Dependency] private readonly SharedStunSystem _stun = default!;
    [Dependency] private readonly SharedXenoHiveSystem _hive = default!;
    [Dependency] private readonly XenoSystem _xeno = default!;
    [Dependency] private readonly RMCPullingSystem _rmcPulling = default!;
    [Dependency] private readonly RMCObstacleSlammingSystem _rmcObstacleSlamming = default!;
    [Dependency] private readonly RMCActionsSystem _rmcActions = default!;
    [Dependency] private readonly MCKnockbackSystem _mcKnockback = default!;

    private EntityQuery<PhysicsComponent> _physicsQuery;
    private EntityQuery<ThrownItemComponent> _thrownItemQuery;

    public override void Initialize()
    {
        _physicsQuery = GetEntityQuery<PhysicsComponent>();
        _thrownItemQuery = GetEntityQuery<ThrownItemComponent>();

        SubscribeLocalEvent<MCXenoLungeComponent, MCXenoLungeActionEvent>(OnXenoLungeAction);
        SubscribeLocalEvent<MCXenoLungeComponent, ThrowDoHitEvent>(OnXenoLungeHit);
        SubscribeLocalEvent<MCXenoLungeComponent, LandEvent>(OnXenoLungeLand);
        SubscribeLocalEvent<MCXenoLungeStunnedComponent, PullStoppedMessage>(OnXenoLungeStunnedPullStopped);
    }

    public override void Update(float frameTime)
    {
        var time = _timing.CurTime;

        var query = EntityQueryEnumerator<MCXenoLungeStunnedComponent>();
        while (query.MoveNext(out var uid, out var stunned))
        {
            if (time < stunned.ExpireAt)
                continue;

            RemCompDeferred<MCXenoLungeStunnedComponent>(uid);
        }
    }

    private void OnXenoLungeAction(Entity<MCXenoLungeComponent> entity, ref MCXenoLungeActionEvent args)
    {
        if (args.Handled)
            return;

        if (!CanAffect(args.Target))
            return;

        if (!_rmcActions.TryUseAction(entity, args.Action, entity))
            return;

        RemComp<MCXenoAgilityActiveComponent>(entity);

        args.Handled = true;

        _rmcPulling.TryStopAllPullsFromAndOn(entity);

        var origin = _transform.GetMapCoordinates(entity);
        var target = _transform.GetMapCoordinates(args.Target);
        var diff = (target.Position - origin.Position).Normalized();

        _rmcObstacleSlamming.MakeImmune(entity, 0.5f);
        _mcKnockback.Knockback(entity, diff, entity.Comp.KnockbackDistance, entity.Comp.KnockbackSpeed, compensateFriction: true, animated: false);

        entity.Comp.Charge = diff;
        entity.Comp.Target = args.Target;
        entity.Comp.Origin = origin;
        Dirty(entity);

        if (!_physicsQuery.TryGetComponent(entity, out var physics))
            return;

        // Handle close-range or same-tile lunges
        foreach (var uid in _physics.GetContactingEntities(entity.Owner, physics))
        {
            if (uid != args.Target)
                continue;

            if (ApplyHitEffects(entity, uid))
                return;
        }
    }

    private void OnXenoLungeHit(Entity<MCXenoLungeComponent> xeno, ref ThrowDoHitEvent args)
    {
        if (_mobState.IsDead(xeno) || HasComp<StunnedComponent>(xeno))
        {
            xeno.Comp.Charge = null;
            xeno.Comp.Target = null;
            return;
        }

        ApplyHitEffects(xeno, args.Target);
    }

    private void OnXenoLungeLand(Entity<MCXenoLungeComponent> ent, ref LandEvent args)
    {
        if (ent.Comp.Charge == null && ent.Comp.Target == null)
            return;

        var target = ent.Comp.Target;
        ent.Comp.Charge = null;
        ent.Comp.Target = null;
        Dirty(ent);

        if (target == null || _pulling.IsPulling(ent))
            return;

        if (_interaction.InRangeUnobstructed(ent.Owner, target.Value))
            ApplyHitEffects(ent, target.Value);
    }

    private bool ApplyHitEffects(Entity<MCXenoLungeComponent> entity, EntityUid uid)
    {
        // TODO: RMC14 lag compensation
        if (!CanAffect(uid))
            return false;

        if (entity.Comp.Charge is null)
            return false;

        if (_physicsQuery.TryGetComponent(entity, out var physics) && _thrownItemQuery.TryGetComponent(entity, out var thrown))
        {
            _thrownItem.LandComponent(entity, thrown, physics, true);
            _thrownItem.StopThrow(entity, thrown);
        }

        if (_timing.IsFirstTimePredicted && entity.Comp.Charge is not null)
            entity.Comp.Charge = null;

        if (_net.IsServer && !_hive.FromSameHive(entity.Owner, uid))
        {
            var stunTime = _xeno.TryApplyXenoDebuffMultiplier(uid, entity.Comp.StunTime);
            _stun.TryParalyze(uid, stunTime, true);

            var stunned = EnsureComp<MCXenoLungeStunnedComponent>(uid);
            stunned.ExpireAt = _timing.CurTime + stunTime;
            stunned.Stunner = GetNetEntity(entity);
            Dirty(uid, stunned);
        }

        _pulling.TryStartPull(entity, uid);
        return true;
    }

    private void OnXenoLungeStunnedPullStopped(Entity<MCXenoLungeStunnedComponent> entity, ref PullStoppedMessage args)
    {
        if (args.PulledUid != entity.Owner)
            return;

        foreach (var effect in entity.Comp.Effects)
        {
            _statusEffects.TryRemoveStatusEffect(entity, effect);
        }
    }

    private bool CanAffect(EntityUid uid)
    {
        if (!HasComp<MarineComponent>(uid) && !HasComp<XenoComponent>(uid))
            return false;

        return !_mobState.IsDead(uid);
    }
}
