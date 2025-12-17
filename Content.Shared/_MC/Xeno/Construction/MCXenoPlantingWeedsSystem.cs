using System.Numerics;
using Content.Shared._RMC14.Actions;
using Content.Shared._RMC14.Xenonids.Hive;
using Content.Shared._RMC14.Xenonids.Plasma;
using Content.Shared._RMC14.Xenonids.Weeds;
using Content.Shared.Actions;
using Content.Shared.Administration.Logs;
using Content.Shared.Coordinates.Helpers;
using Content.Shared.Database;
using Content.Shared.Popups;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;

namespace Content.Shared._MC.Xeno.Construction;

public sealed class MCXenoPlantingWeedsSystem : EntitySystem
{
    [Dependency] private readonly INetManager _net = null!;
    [Dependency] private readonly IMapManager _map = null!;
    [Dependency] private readonly ISharedAdminLogManager _adminLogs = null!;

    [Dependency] private readonly SharedActionsSystem _actions = null!;
    [Dependency] private readonly SharedAudioSystem _audio = null!;
    [Dependency] private readonly SharedTransformSystem _transform = null!;
    [Dependency] private readonly SharedMapSystem _mapSystem = null!;
    [Dependency] private readonly SharedPopupSystem _popup = null!;
    [Dependency] private readonly SharedUserInterfaceSystem _ui = null!;
    [Dependency] private readonly SharedXenoWeedsSystem _xenoWeeds = null!;
    [Dependency] private readonly SharedXenoHiveSystem _xenoHive = null!;
    [Dependency] private readonly XenoPlasmaSystem _xenoPlasma = null!;
    [Dependency] private readonly RMCActionsSystem _rmcActions = null!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MCXenoPlantingWeedsComponent, MCXenoChooseWeedsBuiMsg>(OnChooseMessage);
        SubscribeLocalEvent<MCXenoPlantingWeedsComponent, MCXenoChooseAutoWeedsBuiMsg>(OnChooseAutoMessage);

        SubscribeLocalEvent<MCXenoPlantingWeedsComponent, MCXenoPlaceWeedsActionEvent>(OnPlaceEvent);
        SubscribeLocalEvent<MCXenoPlantingWeedsComponent, MCXenoChooseWeedsActionEvent>(OnChooseEvent);

        SubscribeLocalEvent<MCXenoChooseWeedsActionComponent, MCXenoWeedsChosenEvent>(OnActionWeedsChosen);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (!_net.IsServer)
            return;

        var query = EntityQueryEnumerator<MCXenoPlantingWeedsComponent, XenoPlasmaComponent>();
        while (query.MoveNext(out var entityUid, out var comp, out var plasmaComp))
        {
            if (!comp.Auto)
                continue;

            var coordinates = Transform(entityUid).Coordinates;
            var originPosition = _transform.GetMapCoordinates(entityUid).Position;
            var weedsPosition = comp.LastdWeedsUid is null || !Exists(comp.LastdWeedsUid)
                ? Vector2.Zero
                : Transform(comp.LastdWeedsUid.Value).Coordinates.Position;

            var distance = (originPosition - weedsPosition).Length();
            if (distance < comp.AutoWeedingMinDistance)
                continue;

            if (comp.Selected is not { } weedsSelected)
                continue;

            var weedData = comp.Weeds[weedsSelected];

            if (!_xenoPlasma.HasPlasma((entityUid, plasmaComp), weedData.Cost))
                continue;

            if (_transform.GetGrid(coordinates) is not { } gridUid || !TryComp<MapGridComponent>(gridUid, out var gridComp))
                continue;

            if (_xenoWeeds.IsOnWeeds((gridUid, gridComp), coordinates))
                continue;

            foreach (var action in  _rmcActions.GetActionsWithEvent<MCXenoPlaceWeedsActionEvent>(entityUid))
            {
                if (_actions.IsCooldownActive(action))
                    continue;

                TryPlace((entityUid, comp));
                break;
            }
        }
    }

    private void OnChooseMessage(Entity<MCXenoPlantingWeedsComponent> entity, ref MCXenoChooseWeedsBuiMsg args)
    {
        Select(entity, args.Id);
        _ui.CloseUi(entity.Owner, MCXenoPlantingWeedsUI.Key, entity);
    }

    private void OnChooseAutoMessage(Entity<MCXenoPlantingWeedsComponent> entity, ref MCXenoChooseAutoWeedsBuiMsg args)
    {
        entity.Comp.Auto = args.Value;
        Dirty(entity);

        var text = args.Value
            ? "We will now automatically plant weeds"
            : "We will no longer automatically plant weeds";

        _popup.PopupClient(text, entity, entity);
        _ui.CloseUi(entity.Owner, MCXenoPlantingWeedsUI.Key, entity);
    }

    private void OnChooseEvent(Entity<MCXenoPlantingWeedsComponent> entity, ref MCXenoChooseWeedsActionEvent args)
    {
        args.Handled = true;
        _ui.TryOpenUi(entity.Owner, MCXenoPlantingWeedsUI.Key, entity);
    }

    private void OnPlaceEvent(Entity<MCXenoPlantingWeedsComponent> entity, ref MCXenoPlaceWeedsActionEvent args)
    {
        args.Handled = TryPlace(entity);
    }

    private bool TryPlace(Entity<MCXenoPlantingWeedsComponent> entity)
    {
        if (entity.Comp.Selected is not { } weedsSelected)
            return false;

        var weedsData = entity.Comp.Weeds[weedsSelected];

        var coordinates = _transform.GetMoverCoordinates(entity).SnapToGrid(EntityManager, _map);
        if (_transform.GetGrid(coordinates) is not { } gridUid || !TryComp(gridUid, out MapGridComponent? gridComp))
            return false;

        var grid = new Entity<MapGridComponent>(gridUid, gridComp);
        if (_xenoWeeds.IsOnWeeds(grid, coordinates, true))
        {
            _popup.PopupClient(Loc.GetString("cm-xeno-weeds-source-already-here"), entity.Owner, entity.Owner);
            return false;
        }

        var tile = _mapSystem.CoordinatesToTile(gridUid, gridComp, coordinates);
        if (!_xenoWeeds.CanSpreadWeedsPopup(grid, tile, entity, weedsData.SemiWeedable, true))
            return false;

        if (!_xenoWeeds.CanPlaceWeedsPopup(entity, grid, coordinates, false))
            return false;

        if (!_xenoPlasma.TryRemovePlasmaPopup(entity.Owner, weedsData.Cost))
            return false;

        if (_net.IsClient)
            return true;

        var weeds = Spawn(weedsSelected, coordinates);
        entity.Comp.LastdWeedsUid = weeds;
        Dirty(entity);

        _adminLogs.Add(LogType.RMCXenoPlantWeeds, $"Xeno {ToPrettyString(entity):xeno} planted weeds {ToPrettyString(weeds):weeds} at {coordinates}");
        _xenoHive.SetSameHive(entity.Owner, weeds);

        _audio.PlayPvs(weedsData.PlaceSound, entity);
        return true;
    }

    private void OnActionWeedsChosen(Entity<MCXenoChooseWeedsActionComponent> entity, ref MCXenoWeedsChosenEvent args)
    {
        _actions.SetIcon(entity.Owner, args.Data.Sprite);
    }

    private void Select(Entity<MCXenoPlantingWeedsComponent> entity, EntProtoId id)
    {
        if (!entity.Comp.Weeds.ContainsKey(id))
            return;

        entity.Comp.Selected = id;
        DirtyField(entity, entity.Comp, nameof(MCXenoPlantingWeedsComponent.Selected));

        var ev = new MCXenoWeedsChosenEvent(id, entity.Comp.Weeds[id]);
        foreach (var (entityUid, _) in _actions.GetActions(entity))
        {
            RaiseLocalEvent(entityUid, ref ev);
        }
    }
}
