using Content.Shared._MC.Xeno.Hive.Systems.Main;
using Content.Shared._RMC14.Actions;
using Content.Shared._RMC14.Emote;
using Content.Shared._RMC14.Map;
using Content.Shared._RMC14.Pulling;
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

namespace Content.Shared._MC.Xeno.Abilities.Crusher.Charge;

public sealed partial class MCXenoChargeSystem : EntitySystem
{
     [Dependency] private readonly IRobustRandom _random = null!;
    [Dependency] private readonly SharedActionsSystem _actions = null!;
    [Dependency] private readonly DamageableSystem _damageable = null!;
    [Dependency] private readonly SharedAudioSystem _audio = null!;
    [Dependency] private readonly IConfigurationManager _config = null!;
    [Dependency] private readonly MovementSpeedModifierSystem _movementSpeed = null!;
    [Dependency] private readonly SharedMoverController _moverController = null!;
    [Dependency] private readonly SharedRMCEmoteSystem _rmcEmote = null!;
    [Dependency] private readonly IGameTiming _timing = null!;
    [Dependency] private readonly SharedTransformSystem _transform = null!;
    [Dependency] private readonly XenoPlasmaSystem _xenoPlasma = null!;
    [Dependency] private readonly RMCPullingSystem _rmcPulling = null!;
    [Dependency] private readonly MobStateSystem _mobState = null!;
    [Dependency] private readonly ThrowingSystem _throwing = null!;
    [Dependency] private readonly SharedRMCActionsSystem _rmcActions = null!;
    [Dependency] private readonly MCSharedXenoHiveSystem _mcXenoHive = null!;

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
        SubscribeLocalEvent<MCXenoChargeActiveComponent, MobStateChangedEvent>(OnActiveToggleChargingMobStateChanged);

        SubscribeLocalEvent<MCXenoChargeActiveComponent, MoveInputEvent>(OnActiveToggleChargingMoveInput);
        SubscribeLocalEvent<MCXenoChargeActiveComponent, MoveEvent>(OnActiveToggleChargingMove);
        SubscribeLocalEvent<MCXenoChargeActiveComponent, PreventCollideEvent>(OnActiveToggleChargingCollide);
        SubscribeLocalEvent<MCXenoChargeActiveComponent, RefreshMovementSpeedModifiersEvent>(OnRefreshSpeed);

        SubscribeLocalEvent<DamageableComponent, MCXenoChargeCollideEvent>(OnDamageableHit);


        Subs.CVar(_config, CCVars.RelativeMovement, v => _relativeMovement = v, true);
    }

    public override void Update(float frameTime)
    {
        ProcessHits();
        UpdateActiveChargers();
    }

    private void ProcessHits()
    {
        var time = _timing.CurTime;
        try
        {
            foreach (var hit in _hit)
            {
                if (TerminatingOrDeleted(hit.Crusher) || TerminatingOrDeleted(hit.Target))
                    continue;

                // ИСПРАВЛЕН БАГ: здесь был return, который прерывал весь Update. Заменено на continue.
                if (_xenoToggleChargingRecentlyHitQuery.TryComp(hit.Target, out var recently) &&
                    time < recently.LastHitAt + recently.Cooldown)
                    continue;

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
    }

    private void UpdateActiveChargers()
    {
        var time = _timing.CurTime;
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
}
