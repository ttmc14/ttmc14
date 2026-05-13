using Content.Shared._MC.Engineering.Deploy.Components;
using Content.Shared._MC.Engineering.Deploy.Events;
using Content.Shared._RMC14.Map;
using Content.Shared._RMC14.NPC;
using Content.Shared.DoAfter;
using Content.Shared.Interaction.Events;
using Content.Shared.Item;
using Content.Shared.Popups;
using Content.Shared.Stacks;
using Content.Shared.Verbs;
using Content.Shared.Weapons.Ranged.Systems;
using Robust.Shared.Map;
using Robust.Shared.Network;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Systems;

namespace Content.Shared._MC.Engineering.Deploy;

public sealed class MCDeploySystem : EntitySystem
{
    [Dependency] private readonly INetManager _net = null!;

    [Dependency] private readonly SharedAppearanceSystem _appearance = null!;
    [Dependency] private readonly FixtureSystem _fixture = null!;
    [Dependency] private readonly SharedTransformSystem _transform = null!;
    [Dependency] private readonly SharedPopupSystem _popup = null!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = null!;
    [Dependency] private readonly SharedPhysicsSystem _physics = null!;
    [Dependency] private readonly SharedStackSystem _stack = null!;

    [Dependency] private readonly SharedRMCNPCSystem _rmcNpc = null!;
    [Dependency] private readonly RMCMapSystem _rmcMap = null!;

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
        SubscribeLocalEvent<MCDeployComponent, MCDeployDisassembleDoAfterEvent>(OnDisassembleDoAfter);
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

    public bool Deployed(EntityUid uid)
    {
        if (!_deployQuery.TryComp(uid, out var component))
            return true;

        return component.State == MCDeployState.Deployed;
    }

    private void OnMapInit(Entity<MCDeployComponent> entity, ref MapInitEvent args)
    {
        _toUpdate.Add(entity);
        UpdateState(entity);
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

        if (!CanDeployPopup(entity, args.User, out var coordinates, out var angle))
            return;

        var ev = new MCDeployDoAfterEvent(GetNetCoordinates(coordinates), angle);
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

        var coordinates = GetCoordinates(args.Coordinates);
        var angle = args.Angle;

        if (!CanDeployPopup(entity, args.User, coordinates))
            return;

        if (!_net.IsServer)
            return;

        var targetEntity = entity;
        if (HasComp<StackComponent>(entity) && _stack.Use(entity, 1))
        {
            if (string.IsNullOrEmpty(entity.Comp.DeployedPrototype))
                return;

            var deployedEntity = Spawn(entity.Comp.DeployedPrototype, coordinates);
            targetEntity = new Entity<MCDeployComponent>(deployedEntity, Comp<MCDeployComponent>(deployedEntity));
        }

        _transform.SetCoordinates(targetEntity, Transform(targetEntity), coordinates, angle);
        SetState(targetEntity, MCDeployState.Deployed);
    }

    private void OnDisassembleDoAfter(Entity<MCDeployComponent> entity, ref MCDeployDisassembleDoAfterEvent args)
    {
        var user = args.User;
        if (args.Cancelled || args.Handled || entity.Comp.State == MCDeployState.Item)
            return;

        args.Handled = true;

        SetState(entity, MCDeployState.Item);

        var selfMsg = Loc.GetString("rmc-sentry-disassemble-finish-self", ("sentry", entity));
        var othersMsg = Loc.GetString("rmc-sentry-disassemble-finish-others", ("user", user), ("sentry", entity));
        _popup.PopupPredicted(selfMsg, othersMsg, entity, user);
    }

    private void Disassemble(Entity<MCDeployComponent> entity, EntityUid user)
    {
        if (entity.Comp.State == MCDeployState.Item)
            return;

        var ev = new MCDeployDisassembleDoAfterEvent();
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
                _transform.Unanchor(entity);
                break;

            case MCDeployState.Deployed:
                if (fixture is not null)
                    _physics.SetHard(entity, fixture, true);

                _rmcNpc.WakeNPC(entity);
                _appearance.SetData(entity, MCDeployLayers.Layer, MCDeployState.Deployed);
                _transform.AnchorEntity(entity);
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

        return CanDeployPopup(entity, user, coordinates);
    }

    private bool CanDeployPopup(Entity<MCDeployComponent> entity, EntityUid user, EntityCoordinates coordinates)
    {
        var ev = new MCDeployAttemptEvent(coordinates);
        RaiseLocalEvent(entity, ref ev);

        if (!ev.Cancelled && _rmcMap.CanBuildOn(coordinates))
            return true;

        _popup.PopupClient(Loc.GetString("rmc-sentry-need-open-area", ("sentry", entity)), user, user, PopupType.SmallCaution);
        return false;
    }
}
