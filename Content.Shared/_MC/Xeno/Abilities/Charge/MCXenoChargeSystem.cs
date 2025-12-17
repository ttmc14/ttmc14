using Content.Shared._RMC14.Actions;
using Content.Shared._RMC14.Emote;
using Content.Shared._RMC14.Map;
using Content.Shared._RMC14.Pulling;
using Content.Shared._RMC14.Throwing;
using Content.Shared._RMC14.Xenonids.Hive;
using Content.Shared._RMC14.Xenonids.Plasma;
using Content.Shared.Actions;
using Content.Shared.CCVar;
using Content.Shared.Damage;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Movement.Components;
using Content.Shared.Movement.Events;
using Content.Shared.Movement.Systems;
using Content.Shared.Physics;
using Content.Shared.Throwing;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Configuration;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Events;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Shared._MC.Xeno.Abilities.Charge;

public sealed class MCXenoChargeSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly IConfigurationManager _config = default!;
    [Dependency] private readonly MovementSpeedModifierSystem _movementSpeed = default!;
    [Dependency] private readonly SharedMoverController _moverController = default!;
    [Dependency] private readonly SharedRMCEmoteSystem _rmcEmote = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly XenoPlasmaSystem _xenoPlasma = default!;
    [Dependency] private readonly RMCPullingSystem _rmcPulling = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly SharedXenoHiveSystem _xenoHive = default!;
    [Dependency] private readonly ThrowingSystem _throwing = default!;
    [Dependency] private readonly RMCActionsSystem _rmcActions = default!;

    private EntityQuery<InputMoverComponent> _inputMoverQuery;
    private EntityQuery<MCXenoChargeComponent> _xenoToggleChargingQuery;
    private EntityQuery<MCXenoChargeRecentlyHitComponent> _xenoToggleChargingRecentlyHitQuery;

    private bool _relativeMovement;

    private readonly HashSet<(Entity<MCXenoChargeActiveComponent> Crusher, EntityUid Target)> _hit = new();

    public override void Initialize()
    {
        _inputMoverQuery = GetEntityQuery<InputMoverComponent>();
        _xenoToggleChargingQuery = GetEntityQuery<MCXenoChargeComponent>();
        _xenoToggleChargingRecentlyHitQuery = GetEntityQuery<MCXenoChargeRecentlyHitComponent>();

        SubscribeLocalEvent<MCXenoChargeComponent, MCXenoChargeActionEvent>(OnAction);

        SubscribeLocalEvent<MCXenoChargeActiveComponent, MapInitEvent>(OnActiveInit);
        SubscribeLocalEvent<MCXenoChargeActiveComponent, ComponentRemove>(OnActiveRemove);

        SubscribeLocalEvent<MCXenoChargeActiveComponent, RefreshMovementSpeedModifiersEvent>(OnRefreshSpeed);

        SubscribeLocalEvent<MCXenoChargeActiveComponent, MoveInputEvent>(OnActiveToggleChargingMoveInput);
        SubscribeLocalEvent<MCXenoChargeActiveComponent, MoveEvent>(OnActiveToggleChargingMove);
        SubscribeLocalEvent<MCXenoChargeActiveComponent, PreventCollideEvent>(OnActiveToggleChargingCollide);
        SubscribeLocalEvent<MCXenoChargeActiveComponent, MobStateChangedEvent>(OnActiveToggleChargingMobStateChanged);

        SubscribeLocalEvent<DamageableComponent, MCXenoChargeCollideEvent>(OnDamageableHit);

        Subs.CVar(_config, CCVars.RelativeMovement, v => _relativeMovement = v, true);
    }

    public override void Update(float frameTime)
    {
        var time = _timing.CurTime;
        try
        {
            foreach (var hit in _hit)
            {
                if (TerminatingOrDeleted(hit.Crusher) || TerminatingOrDeleted(hit.Target))
                    continue;

                if (_xenoToggleChargingRecentlyHitQuery.TryComp(hit.Target, out var recently) && time < recently.LastHitAt + recently.Cooldown)
                    return;

                var ev = new MCXenoChargeCollideEvent(hit.Crusher);
                RaiseLocalEvent(hit.Target, ref ev);

                if (!ev.Handled)
                    continue;

                recently = EnsureComp<MCXenoChargeRecentlyHitComponent>(hit.Target);
                recently.LastHitAt = time;
                Dirty(hit.Target, recently);

                if (hit.Crusher.Comp.Stage != 0)
                    continue;

                hit.Crusher.Comp.Steps = 0;
                Dirty(hit.Crusher);
            }
        }
        finally
        {
            _hit.Clear();
        }

        var query = EntityQueryEnumerator<MCXenoChargeActiveComponent, MCXenoChargeComponent, PhysicsComponent>();
        while (query.MoveNext(out var uid, out var active, out var charging, out var physics))
        {
            if (physics.BodyStatus == BodyStatus.InAir)
            {
                ResetCharging((uid, active));
                continue;
            }

            if (time >= active.LastMovedAt + charging.LastMovedGrace)
                ResetCharging((uid, active), false);
        }
    }

    private void OnAction(Entity<MCXenoChargeComponent> entity, ref MCXenoChargeActionEvent args)
    {
        if (_timing.ApplyingState)
            return;

        if (RemComp<MCXenoChargeActiveComponent>(entity))
            return;

        if (!TryComp<InputMoverComponent>(entity, out var mover))
            return;

        var direction = GetHeldButton(entity, mover.HeldMoveButtons);
        var active = new MCXenoChargeActiveComponent();
        AddComp(entity, active, true);

        // Moving diagonally
        if ((direction & (direction - 1)) != DirectionFlag.None)
            return;

        active.Direction = direction;
        Dirty(entity, active);
    }

    private void OnActiveInit(Entity<MCXenoChargeActiveComponent> entity, ref MapInitEvent args)
    {
        _movementSpeed.RefreshMovementSpeedModifiers(entity);
        foreach (var action in  _rmcActions.GetActionsWithEvent<MCXenoChargeActionEvent>(entity))
        {
            _actions.SetToggled((action, action), true);
        }
    }

    private void OnActiveRemove(Entity<MCXenoChargeActiveComponent> entity, ref ComponentRemove args)
    {
        _movementSpeed.RefreshMovementSpeedModifiers(entity);
        foreach (var action in  _rmcActions.GetActionsWithEvent<MCXenoChargeActionEvent>(entity))
        {
            _actions.SetToggled((action, action), false);
        }
    }

    private void OnRefreshSpeed(Entity<MCXenoChargeActiveComponent> entity, ref RefreshMovementSpeedModifiersEvent args)
    {
        if (entity.Comp.Stage == 0)
            return;

        if (!_xenoToggleChargingQuery.TryComp(entity, out var charging))
            return;

        args.ModifySpeed(1 + entity.Comp.Stage * charging.SpeedPerStage);
    }

    private void OnActiveToggleChargingMoveInput(Entity<MCXenoChargeActiveComponent> ent, ref MoveInputEvent args)
    {
        var direction = GetHeldButton(ent, args.Entity.Comp.HeldMoveButtons & MoveButtons.AnyDirection);

        // Same direction and not diagonal
        if (direction != DirectionFlag.None &&
            (ent.Comp.Direction & direction) == direction &&
            (direction & (direction - 1)) == DirectionFlag.None)
        {
            return;
        }

        if (ent.Comp.Direction != DirectionFlag.None)
        {
            var perpendiculars = ent.Comp.Direction.AsDir().GetPerpendiculars();
            var isPerpendicular = ent.Comp.Direction == perpendiculars.First.AsFlag() ||
                                  ent.Comp.Direction == perpendiculars.Second.AsFlag();

            if (isPerpendicular &&
                (ent.Comp.Deviated == DirectionFlag.None || ent.Comp.Deviated == direction))
            {
                ent.Comp.Deviated = direction;
                return;
            }
        }

        ResetCharging(ent);
        ent.Comp.Direction = direction;
    }

    private void OnActiveToggleChargingMove(Entity<MCXenoChargeActiveComponent> ent, ref MoveEvent args)
    {
        if (!_xenoToggleChargingQuery.TryComp(ent, out var charging))
            return;

        if (_rmcPulling.IsBeingPulled(ent.Owner, out _))
            return;

        if (!args.OldPosition.TryDistance(EntityManager, _transform, args.NewPosition, out var distance))
            return;

        var absDistance = Math.Abs(distance);
        ent.Comp.Distance += absDistance;
        ent.Comp.LastMovedAt = _timing.CurTime;
        Dirty(ent);

        if (_inputMoverQuery.TryComp(ent, out var mover))
        {
            var lastRotation = ent.Comp.LastRelativeRotation;
            ent.Comp.LastRelativeRotation = mover.RelativeRotation;
            if (ent.Comp.LastRelativeRotation != lastRotation)
            {
                ResetStage(ent);
                return;
            }
        }

        if (ent.Comp.Deviated != DirectionFlag.None)
        {
            ent.Comp.DeviatedDistance += absDistance;
            if (ent.Comp.DeviatedDistance >= charging.MaxDeviation)
            {
                ResetCharging(ent);
                return;
            }
        }

        if (ent.Comp.Distance < charging.StepIncrement)
            return;

        ent.Comp.Steps += charging.StepIncrement;
        ent.Comp.Distance -= charging.StepIncrement;

        if (ent.Comp.Steps < charging.MinimumSteps)
            return;

        var plasmaConsume = ent.Comp.Stage * charging.SpeedPerStage * charging.PlasmaUseMultiplier;
        if (!_xenoPlasma.TryRemovePlasma(ent.Owner, plasmaConsume))
        {
            ResetCharging(ent, false);
            return;
        }

        _rmcPulling.TryStopAllPullsFromAndOn(ent);
        if (ent.Comp.Stage == charging.MaxStage - 1 &&
            charging.Emote is { } emote)
        {
            _rmcEmote.TryEmoteWithChat(ent, emote, cooldown: charging.EmoteCooldown);
        }

        ent.Comp.Stage = Math.Min(charging.MaxStage, ent.Comp.Stage + 1);
        ent.Comp.SoundSteps += charging.StepIncrement;

        if (ent.Comp.Stage == 1 || ent.Comp.SoundSteps >= charging.SoundEvery)
        {
            ent.Comp.SoundSteps = 0;
            if (_timing.InSimulation)
                _audio.PlayPredicted(charging.Sound, ent, ent);
        }

        Dirty(ent);
        _movementSpeed.RefreshMovementSpeedModifiers(ent);
    }

    private void OnActiveToggleChargingCollide(Entity<MCXenoChargeActiveComponent> ent, ref PreventCollideEvent args)
    {
        if (Math.Abs(ent.Comp.Steps - 1) < 0.001)
            return;

        if (args.OtherFixture.CollisionLayer == (int) CollisionGroup.SlipLayer)
            return;

        _hit.Add((ent, args.OtherEntity));

        if (HasComp<MobStateComponent>(args.OtherEntity))
            args.Cancelled = true;
    }

    private void OnActiveToggleChargingMobStateChanged(Entity<MCXenoChargeActiveComponent> ent, ref MobStateChangedEvent args)
    {
        if (args.NewMobState == MobState.Alive)
            return;

        ResetCharging(ent);

        if (_timing.ApplyingState)
            return;

        RemComp<MCXenoChargeActiveComponent>(ent);
    }

    private void OnDamageableHit(Entity<DamageableComponent> entity, ref MCXenoChargeCollideEvent args)
    {
        args.Handled = true;

        var charger = Comp<MCXenoChargeComponent>(args.Charger);
        if (args.Charger.Comp.Stage < charger.MinimumSteps)
            return;

        var perpendiculars = args.Charger.Comp.Direction.AsDir().GetPerpendiculars();
        var perpendicular = _random.Prob(0.5f) ? perpendiculars.First : perpendiculars.Second;
        var direction = perpendicular.ToVec().Normalized();

        var transform = Transform(entity);
        if (transform.Anchored)
        {
            if (_xenoHive.FromSameHive(entity.Owner, args.Charger.Owner))
                return;

            _damageable.TryChangeDamage(entity, charger.StructureDamage * args.Charger.Comp.Stage * charger.SpeedPerStage);
            IncrementStages(args.Charger, -2);
            return;
        }

        if (HasComp<MobStateComponent>(entity) && !_mobState.IsDead(entity))
        {
            _throwing.TryThrow(entity, direction, 5f);

            if (_xenoHive.FromSameHive(entity.Owner, args.Charger.Owner))
            {
                IncrementStages(args.Charger, -1);
                return;
            }

            _damageable.TryChangeDamage(entity, charger.Damage * args.Charger.Comp.Stage * charger.SpeedPerStage);
            return;
        }
    }

    private void ResetCharging(Entity<MCXenoChargeActiveComponent> xeno, bool resetInput = true)
    {
        ResetStage(xeno);
        xeno.Comp.DeviatedDistance = 0;

        if (resetInput)
            xeno.Comp.Direction = DirectionFlag.None;

        Dirty(xeno);
        _movementSpeed.RefreshMovementSpeedModifiers(xeno);
    }

    private void ResetStage(Entity<MCXenoChargeActiveComponent> xeno)
    {
        xeno.Comp.Steps = 0;
        xeno.Comp.SoundSteps = 0;
        xeno.Comp.Stage = 0;

        Dirty(xeno);
        _movementSpeed.RefreshMovementSpeedModifiers(xeno);
    }

    private void IncrementStages(Entity<MCXenoChargeActiveComponent> ent, int increment)
    {
        ent.Comp.Stage = Math.Max(0, ent.Comp.Stage + increment);

        if (_xenoToggleChargingQuery.TryComp(ent, out var charging))
            ent.Comp.Stage = Math.Min(charging.MaxStage, ent.Comp.Stage);

        Dirty(ent);
        _movementSpeed.RefreshMovementSpeedModifiers(ent);
    }

    private DirectionFlag GetHeldButton(EntityUid mover, MoveButtons button)
    {
        if (!TryComp<InputMoverComponent>(mover, out var moverComp))
            return DirectionFlag.None;

        var parentRotation = _moverController.GetParentGridAngle(moverComp);
        var total = _moverController.DirVecForButtons(button);
        var wishDir = _relativeMovement ? parentRotation.RotateVec(total) : total;
        return wishDir.GetDir().AsFlag();
    }
}
