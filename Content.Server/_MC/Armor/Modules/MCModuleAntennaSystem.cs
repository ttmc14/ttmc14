using Content.Shared._MC.Areas;
using Content.Shared._MC.Armor.Modules.Core.Components;
using Content.Shared._MC.Armor.Modules.Core.Events;
using Content.Shared._MC.Armor.Modules.Features.Antenna;
using Content.Shared._MC.Engineering.Beacon.Components;
using Content.Shared._RMC14.Dropship;
using Content.Shared._RMC14.Map;
using Content.Shared.Actions;
using Content.Shared.Actions.Components;
using Content.Shared.Inventory;
using Content.Shared.Maps;
using Content.Shared.Popups;
using Robust.Shared.Containers;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;

namespace Content.Server._MC.Armor.Modules;

public sealed class MCModuleAntennaSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actions = null!;
    [Dependency] private readonly SharedContainerSystem _containers = null!;
    [Dependency] private readonly MCAreasSystem _areas = null!;
    [Dependency] private readonly InventorySystem _inventory = null!;
    [Dependency] private readonly RMCMapSystem _map = null!;
    [Dependency] private readonly MetaDataSystem _meta = null!;
    [Dependency] private readonly SharedPopupSystem _popup = null!;
    [Dependency] private readonly SharedTransformSystem _transform = null!;
    [Dependency] private readonly TurfSystem _turf = null!;

    private static readonly EntProtoId SignalPrototype = "MCHelmetAntennaSignal";

    public override void Initialize()
    {
        SubscribeLocalEvent<MCModuleAntennaComponent, MCArmorModuleUserChangedEvent>(OnUserChanged);
        SubscribeLocalEvent<MCModuleAntennaComponent, EntGotInsertedIntoContainerMessage>(OnInserted);
        SubscribeLocalEvent<MCModuleAntennaComponent, EntGotRemovedFromContainerMessage>(OnRemoved);
        SubscribeLocalEvent<MCModuleAntennaComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<MCModuleAntennaComponent, MapUidChangedEvent>(OnMapChanged);
        SubscribeLocalEvent<MCModuleAntennaComponent, MetaFlagRemoveAttemptEvent>(OnFlagRemoveAttempt);
        SubscribeLocalEvent<MCModuleAntennaComponent, MCModuleAntennaActionEvent>(OnToggle);
    }

    private void OnUserChanged(Entity<MCModuleAntennaComponent> ent, ref MCArmorModuleUserChangedEvent args)
    {
        RefreshAction(ent, args.NewUser);
    }

    private void OnInserted(Entity<MCModuleAntennaComponent> ent, ref EntGotInsertedIntoContainerMessage args)
    {
        if (TryComp<MCArmorModularClothingComponent>(args.Container.Owner, out var armor) && armor.ContainerId == args.Container.ID)
            RefreshAction(ent, armor.CurrentUser);
    }

    private void OnRemoved(Entity<MCModuleAntennaComponent> ent, ref EntGotRemovedFromContainerMessage args)
    {
        RefreshAction(ent, null);
    }

    private void OnShutdown(Entity<MCModuleAntennaComponent> ent, ref ComponentShutdown args)
    {
        RefreshAction(ent, null);
    }

    private void OnMapChanged(Entity<MCModuleAntennaComponent> ent, ref MapUidChangedEvent args)
    {
        Disable(ent);
    }

    private void OnFlagRemoveAttempt(Entity<MCModuleAntennaComponent> ent, ref MetaFlagRemoveAttemptEvent args)
    {
        if (ent.Comp.Signal != null)
            args.ToRemove &= ~MetaDataFlags.ExtraTransformEvents;
    }

    private bool IsWorn(EntityUid module, EntityUid user)
    {
        return _inventory.TryGetSlotEntity(user, "head", out var helmet) &&
            TryComp<MCArmorModularClothingComponent>(helmet, out var armor) &&
            _containers.TryGetContainingContainer(module, out var slot) &&
            slot.Owner == helmet && slot.ID == armor.ContainerId;
    }

    private void RefreshAction(Entity<MCModuleAntennaComponent> ent, EntityUid? wearer)
    {
        if (wearer is { } candidate && !IsWorn(ent, candidate))
            wearer = null;

        if (wearer != null && TryComp<ActionComponent>(ent.Comp.ActionEntity, out var action) && action.AttachedEntity == wearer)
            return;

        Disable(ent);
        if (ent.Comp.ActionEntity is { } oldAction)
        {
            ent.Comp.ActionEntity = null;
            _actions.RemoveAction(oldAction);
        }

        if (wearer is { } user)
            _actions.AddAction(user, ref ent.Comp.ActionEntity, ent.Comp.Action, ent);

        if (ent.Comp.LifeStage < ComponentLifeStage.Stopping)
            Dirty(ent);
    }

    private void OnToggle(Entity<MCModuleAntennaComponent> ent, ref MCModuleAntennaActionEvent args)
    {
        if (args.Handled || args.Action.Owner != ent.Comp.ActionEntity || !IsWorn(ent, args.Performer))
            return;

        args.Handled = true;
        var user = args.Performer;
        if (ent.Comp.Signal != null)
        {
            Disable(ent);
            _popup.PopupEntity(Loc.GetString("mc-antenna-disabled"), user, user);
            return;
        }

        if (!TryGetSignalTile(user, out var tile))
        {
            _popup.PopupEntity(Loc.GetString("mc-antenna-no-signal"), user, user);
            return;
        }

        var area = _areas.GetAreaName(user);
        var signal = Spawn(SignalPrototype, _turf.GetTileCenter(tile));
        ent.Comp.Signal = signal;
        _meta.AddFlag(ent, MetaDataFlags.ExtraTransformEvents);
        Dirty(ent);
        AddComp(signal, new MCBeaconComponent
        {
            Category = "Supply",
            Name = Loc.GetString("mc-asrs-beacon-name", ("user", Name(user)), ("area", area)),
        });
        _actions.SetToggled(ent.Comp.ActionEntity, true);
        _popup.PopupEntity(Loc.GetString("mc-antenna-enabled", ("area", area),
            ("x", tile.GridIndices.X), ("y", tile.GridIndices.Y)), user, user);
    }

    private bool TryGetSignalTile(EntityUid source, out TileRef tile)
    {
        var coords = _transform.GetMoverCoordinates(source);
        return _map.TryGetTileRefForEnt(coords, out var grid, out tile) && !_turf.IsSpace(tile) &&
            !HasComp<DropshipComponent>(grid) && !HasComp<MCBlockAntennaSignalComponent>(grid) &&
            !_areas.AreaHas<MCBlockAntennaSignalComponent>(coords);
    }

    private void Disable(Entity<MCModuleAntennaComponent> ent)
    {
        if (ent.Comp.Signal is not { } signal)
            return;

        ent.Comp.Signal = null;
        _meta.RemoveFlag(ent, MetaDataFlags.ExtraTransformEvents);
        if (ent.Comp.LifeStage < ComponentLifeStage.Stopping)
            Dirty(ent);
        _actions.SetToggled(ent.Comp.ActionEntity, false);
        if (TerminatingOrDeleted(signal))
            return;

        // Invalidate the destination immediately. The grid may still be moving its children.
        RemComp<MCBeaconComponent>(signal);
        QueueDel(signal);
    }

    public override void Update(float frameTime)
    {
        var query = EntityQueryEnumerator<MCModuleAntennaComponent>();
        while (query.MoveNext(out var uid, out var antenna))
        {
            if (antenna.Signal is { } signal && !HasComp<MCBeaconComponent>(signal))
                Disable((uid, antenna));
        }
    }
}
