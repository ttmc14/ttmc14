using System.Numerics;
using Content.Shared._MC.Droppod.Components;
using Content.Shared._MC.Droppod.Events;
using Content.Shared._MC.Operation;
using Content.Shared._MC.Operation.Events;
using Content.Shared._RMC14.Explosion;
using Content.Shared._RMC14.Rules;
using Content.Shared.Actions;
using Content.Shared.Buckle.Components;
using Content.Shared.Parallax;
using Content.Shared.Popups;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Timing;

namespace Content.Shared._MC.Droppod;

public abstract class MCSharedDroppodSystem : EntitySystem
{
    [Dependency] private readonly MCOperationSystem _mcOperation = default!;
    [Dependency] private readonly MetaDataSystem _metaData = default!;
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly SharedMapSystem _map = default!;
    [Dependency] private readonly SharedRMCExplosionSystem _explosion = default!;
    [Dependency] private readonly SharedUserInterfaceSystem _ui = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MCOperationStartEvent>(OnOperationStart);

        SubscribeLocalEvent<MCDroppodComponent, ComponentStartup>(OnStartup);

        SubscribeLocalEvent<MCDroppodComponent, StrappedEvent>(OnStrapped);
        SubscribeLocalEvent<MCDroppodComponent, UnstrappedEvent>(OnUnstrapped);
        SubscribeLocalEvent<MCDroppodComponent, StrapAttemptEvent>(OnStrapAttempt);
        SubscribeLocalEvent<MCDroppodComponent, UnstrapAttemptEvent>(OnUnstrapAttempt);
        SubscribeLocalEvent<MCDroppodComponent, MCDroppodLaunchActionEvent>(OnLaunch);
        SubscribeLocalEvent<MCDroppodComponent, MCDroppodTargetActionEvent>(OnTaget);
        SubscribeLocalEvent<MCDroppodComponent, MCDroppodTagetBuiMsg>(OnTargetMessage);

        SubscribeLocalEvent<MCDroppodUserComponent, MCDroppodLaunchActionEvent>(RelayEvent);
        SubscribeLocalEvent<MCDroppodUserComponent, MCDroppodTargetActionEvent>(RelayEvent);
    }

    private void OnOperationStart(MCOperationStartEvent ev)
    {
        var query = EntityQueryEnumerator<MCDroppodComponent>();
        while (query.MoveNext(out var uid, out var component))
        {
            if (component.OperationStarted)
                continue;

            component.OperationStarted = true;
            Dirty(uid, component);
        }
    }

    private void OnStartup(Entity<MCDroppodComponent> entity, ref ComponentStartup args)
    {
        entity.Comp.OperationStarted = _mcOperation.Started;
        Dirty(entity);

        UpdateSprite(entity);
    }

    private void OnStrapped(Entity<MCDroppodComponent> entity, ref StrappedEvent args)
    {
        foreach (var id in entity.Comp.Actions)
        {
            if (_actions.AddAction(args.Buckle, id) is not { } action)
                continue;

            entity.Comp.ActionEntities.Add(action);
        }

        Dirty(entity);

        var userComponent = EnsureComp<MCDroppodUserComponent>(args.Buckle);
        userComponent.DroppodEntity = entity;
        Dirty(args.Buckle, userComponent);
    }

    private void OnUnstrapped(Entity<MCDroppodComponent> entity, ref UnstrappedEvent args)
    {
        foreach (var actionEntity in entity.Comp.ActionEntities)
        {
            _actions.RemoveAction(actionEntity);
        }

        entity.Comp.ActionEntities.Clear();
        Dirty(entity);

        RemComp<MCDroppodUserComponent>(args.Buckle);
    }

    private void OnStrapAttempt(Entity<MCDroppodComponent> entity, ref StrapAttemptEvent args)
    {
        if (entity.Comp.State == MCDroppodState.Ready)
            return;

        args.Cancelled = true;
    }

    private void OnUnstrapAttempt(Entity<MCDroppodComponent> entity, ref UnstrapAttemptEvent args)
    {
        if (entity.Comp.State != MCDroppodState.Active)
            return;

        args.Cancelled = true;
    }

    private void OnLaunch(Entity<MCDroppodComponent> entity, ref MCDroppodLaunchActionEvent args)
    {
        if (!entity.Comp.OperationStarted)
        {
            _popup.PopupClient("Unable to launch, the ship has not yet reached the combat area.", args.Performer, args.Performer);
            return;
        }

        if (!HasLauncher(entity))
        {
            _popup.PopupClient("Error. Cannot launch droppod without a launcher.", args.Performer, args.Performer);
            return;
        }

        if (!entity.Comp.LaunchAllowed)
        {
            _popup.PopupClient("Error. Ship calibration unavailable. Please %#&ç:*", args.Performer, args.Performer);
            return;
        }

        if (entity.Comp.State != MCDroppodState.Ready)
            return;

        entity.Comp.State = MCDroppodState.Active;
        UpdateSprite(entity);
        Dirty(entity);

        Timer.Spawn(TimeSpan.FromSeconds(2.5f), () => Launch(entity));
    }

    private void OnTaget(Entity<MCDroppodComponent> entity, ref MCDroppodTargetActionEvent args)
    {
        _ui.OpenUi(entity.Owner, MCDroppodUI.Key, args.Performer);
    }

    private void OnTargetMessage(Entity<MCDroppodComponent> entity, ref MCDroppodTagetBuiMsg args)
    {
        if (GetPlanetMap() is not { } mapId)
            return;

        if (!_map.TryGetMap(mapId, out var mapEntity))
            return;

        if (!TryComp<MapGridComponent>(mapEntity, out var gridComponent))
            return;

        var position = _transform.ToMapCoordinates(new EntityCoordinates(mapEntity.Value, (Vector2) args.Tile * gridComponent.TileSize));
        entity.Comp.Target = new MapCoordinates(position.Position, mapId);
        Dirty(entity);
    }

    protected virtual void UpdateSprite(Entity<MCDroppodComponent> entity)
    {

    }

    private void Launch(Entity<MCDroppodComponent> entity)
    {
        var map = GetTransferMap();
        var coordinates = new MapCoordinates(_transform.GetWorldPosition(entity), map);

        _transform.SetMapCoordinates(entity, coordinates);

        Timer.Spawn(entity.Comp.TransitDelay, () => Finish(entity));
    }

    private void Finish(Entity<MCDroppodComponent> entity)
    {
        Timer.Spawn(entity.Comp.FallDelay, () => Drop(entity));
    }

    private void Drop(Entity<MCDroppodComponent> entity)
    {
        if (entity.Comp.Target is not { } target)
            return;

        _explosion.QueueExplosion(target, "RMC", 110, 40, 10, entity);

        Timer.Spawn(
            entity.Comp.FallDelay,
            () =>
            {
                _transform.SetMapCoordinates(entity, target);

                entity.Comp.State = MCDroppodState.Landed;
                Dirty(entity);

                UpdateSprite(entity);
            }
        );
    }

    private MapId GetTransferMap()
    {
        var query = EntityQueryEnumerator<MCDroppodTransferMapComponent, MapComponent>();
        while (query.MoveNext(out _, out _, out var mapComponent))
        {
            return mapComponent.MapId;
        }

        var mapUid = _map.CreateMap(out var mapId);
        var ftlMap = AddComp<MCDroppodTransferMapComponent>(mapUid);

        _metaData.SetEntityName(mapUid, "Droppod transfer");

        var parallax = EnsureComp<ParallaxComponent>(mapUid);
        parallax.Parallax = ftlMap.Parallax;

        return mapId;
    }

    private MapId? GetPlanetMap()
    {
        var query = EntityQueryEnumerator<RMCPlanetComponent, MapComponent>();
        while (query.MoveNext(out _, out _, out var mapComponent))
        {
            return mapComponent.MapId;
        }

        return null;
    }

    private void RelayEvent<T>(Entity<MCDroppodUserComponent> entity, ref T args) where T : BaseActionEvent
    {
        if (!Exists(entity.Comp.DroppodEntity))
        {
            Log.Error("Fuck!");
            return;
        }

        if (!HasComp<MCDroppodComponent>(entity.Comp.DroppodEntity))
        {
            Log.Error("Fuck!");
            return;
        }

        RaiseLocalEvent(entity.Comp.DroppodEntity, args);
    }

    private bool HasLauncher(Entity<MCDroppodComponent> entity)
    {
        var coords = Transform(entity).Coordinates;
        if (_transform.GetGrid(coords) is not { } gridId || !TryComp<MapGridComponent>(gridId, out var grid))
            return false;

        var tile = _map.TileIndicesFor(gridId, grid, coords);
        var anchored = _map.GetAnchoredEntitiesEnumerator(gridId, grid, tile);

        while (anchored.MoveNext(out var uid))
        {
            if (HasComp<MCDroppodLauncherComponent>(uid))
                return true;
        }

        return false;
    }
}
