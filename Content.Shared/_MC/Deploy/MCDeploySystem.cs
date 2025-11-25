using Content.Shared._MC.Deploy.Events;
using Content.Shared._RMC14.Map;
using Content.Shared._RMC14.NPC;
using Content.Shared.DoAfter;
using Content.Shared.Interaction.Events;
using Content.Shared.Item;
using Content.Shared.Popups;
using Content.Shared.Verbs;
using Content.Shared.Weapons.Ranged.Systems;
using Robust.Shared.Map;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Systems;

namespace Content.Shared._MC.Deploy;

public sealed class MCDeploySystem : EntitySystem
{
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly FixtureSystem _fixture = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly SharedPhysicsSystem _physics = default!;

    [Dependency] private readonly SharedRMCNPCSystem _rmcNpc = default!;
    [Dependency] private readonly RMCMapSystem _rmcMap = default!;

    private readonly HashSet<EntityUid> _toUpdate = new();

    private EntityQuery<MCDeployComponent> _deployQuery;

    public override void Initialize()
    {
        base.Initialize();

        _deployQuery = GetEntityQuery<MCDeployComponent>();

        SubscribeLocalEvent<MCDeployComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<MCDeployComponent, GetVerbsEvent<AlternativeVerb>>(OnGetVerbs);
        SubscribeLocalEvent<MCDeployComponent, PickupAttemptEvent>(OnPickupAttempt);
        SubscribeLocalEvent<MCDeployComponent, AttemptShootEvent>(OnAttemptShoot);
        SubscribeLocalEvent<MCDeployComponent, UseInHandEvent>(OnUseInHand);
        SubscribeLocalEvent<MCDeployComponent, MCDeployDoAfterEvent>(OnDeployDoAfter);
        SubscribeLocalEvent<MCDeployComponent, MCDisassembleDoAfterEvent>(OnDisassembleDoAfter);
    }

    private void OnMapInit(Entity<MCDeployComponent> entity, ref MapInitEvent args)
    {
        _toUpdate.Add(entity);
        UpdateState(entity);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        foreach (var uid in _toUpdate)
        {
            if (!_deployQuery.TryComp(uid, out var sentryComponent))
                continue;

            UpdateState((uid, sentryComponent));
        }

        _toUpdate.Clear();
    }


    private void OnGetVerbs(Entity<MCDeployComponent> entity, ref GetVerbsEvent<AlternativeVerb> args)
    {
        if (!args.CanInteract || !args.CanAccess)
            return;

        var user = args.User;
        args.Verbs.Add(new AlternativeVerb
        {
            Text = Loc.GetString("Disassemble"),
            Act = () =>
            {
                Disassemble(entity, user);
            },
            Priority = 9999,
        });

    }

    private void OnPickupAttempt(Entity<MCDeployComponent> entity, ref PickupAttemptEvent args)
    {
        if (args.Cancelled)
            return;

        if (entity.Comp.State == MCDeployState.Item)
            return;

        args.Cancel();
    }

    private void OnAttemptShoot(Entity<MCDeployComponent> entity, ref AttemptShootEvent args)
    {
        // Since the turret is folded and deployed as a single entity,
        // we prohibit shooting from the hands
        if (!args.Cancelled && args.User != entity.Owner)
            args.Cancelled = true;
    }

    private void OnUseInHand(Entity<MCDeployComponent> entity, ref UseInHandEvent args)
    {
        args.Handled = true;

        if (!CanDeployPopup(entity, args.User, out _, out _))
            return;

        var ev = new MCDeployDoAfterEvent();
        var delay = entity.Comp.DeployTime;
        var doAfter = new DoAfterArgs(EntityManager, args.User, delay, ev, entity, entity, entity)
        {
            BreakOnMove = true,
            BreakOnDropItem = true,
        };

        _doAfter.TryStartDoAfter(doAfter);
    }

    private void OnDeployDoAfter(Entity<MCDeployComponent> entity, ref MCDeployDoAfterEvent args)
    {
        if (args.Cancelled || args.Handled || entity.Comp.State == MCDeployState.Deployed)
            return;

        args.Handled = true;
        if (!CanDeployPopup(entity, args.User, out var coordinates, out var angle))
            return;

        SetState(entity, MCDeployState.Deployed);

        var xform = Transform(entity);
        _transform.SetCoordinates(entity, xform, coordinates, angle);
        _transform.AnchorEntity(entity, xform);
    }

    private void OnDisassembleDoAfter(Entity<MCDeployComponent> entity, ref MCDisassembleDoAfterEvent args)
    {
        var user = args.User;
        if (args.Cancelled || args.Handled || entity.Comp.State == MCDeployState.Item)
            return;

        args.Handled = true;

        SetState(entity, MCDeployState.Item);

        _transform.Unanchor(entity.Owner, Transform(entity));

        var selfMsg = Loc.GetString("rmc-sentry-disassemble-finish-self", ("sentry", entity));
        var othersMsg = Loc.GetString("rmc-sentry-disassemble-finish-others", ("user", user), ("sentry", entity));
        _popup.PopupPredicted(selfMsg, othersMsg, entity, user);
    }

    private void Disassemble(Entity<MCDeployComponent> entity, EntityUid user)
    {
        if (entity.Comp.State == MCDeployState.Item)
            return;

        var ev = new MCDisassembleDoAfterEvent();
        var delay = entity.Comp.DeployTime;

        var doAfter = new DoAfterArgs(EntityManager, user, delay, ev, entity)
        {
            BreakOnMove = true,
        };

        _doAfter.TryStartDoAfter(doAfter);
    }

    private void SetState(Entity<MCDeployComponent> entity, MCDeployState state)
    {
        var previousState = entity.Comp.State;

        entity.Comp.State = state;
        Dirty(entity);

        var ev  = new MCDeployChangedStateEvent(state, previousState);
        RaiseLocalEvent(entity, ref ev);

        UpdateState(entity);
    }

    private void UpdateState(Entity<MCDeployComponent> entity)
    {
        var fixture = entity.Comp.DeployFixture is { } fixtureId && TryComp<FixturesComponent>(entity, out var fixtures)
            ? _fixture.GetFixtureOrNull(entity, fixtureId, fixtures)
            : null;

        switch (entity.Comp.State)
        {
            case MCDeployState.Item:
                if (fixture is not null)
                    _physics.SetHard(entity, fixture, false);

                _rmcNpc.SleepNPC(entity);
                _appearance.SetData(entity, MCDeployLayers.Layer, MCDeployState.Item);
                break;

            case MCDeployState.Deployed:
                if (fixture is not null)
                    _physics.SetHard(entity, fixture, true);

                _rmcNpc.WakeNPC(entity);
                _appearance.SetData(entity, MCDeployLayers.Layer, MCDeployState.Deployed);
                break;

            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private bool CanDeployPopup(
        Entity<MCDeployComponent> entity,
        EntityUid user,
        out EntityCoordinates coordinates,
        out Angle rotation)
    {
        coordinates = default;
        rotation = default;

        var moverCoordinates = _transform.GetMoverCoordinateRotation(user, Transform(user));
        coordinates = moverCoordinates.Coords;
        rotation = moverCoordinates.worldRot.GetCardinalDir().ToAngle();

        var direction = rotation.GetCardinalDir();
        coordinates = coordinates.Offset(direction.ToVec());

        if (_rmcMap.CanBuildOn(coordinates))
            return true;

        _popup.PopupClient(Loc.GetString("rmc-sentry-need-open-area", ("sentry", entity)), user, user, PopupType.SmallCaution);
        return false;
    }
}
