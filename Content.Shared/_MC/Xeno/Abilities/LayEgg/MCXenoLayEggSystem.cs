using Content.Shared._RMC14.Actions;
using Content.Shared._RMC14.Xenonids.Construction;
using Content.Shared._RMC14.Xenonids.Construction.Tunnel;
using Content.Shared._RMC14.Xenonids.Egg;
using Content.Shared._RMC14.Xenonids.Hive;
using Content.Shared._RMC14.Xenonids.Weeds;
using Content.Shared.Actions;
using Content.Shared.Buckle.Components;
using Content.Shared.DoAfter;
using Content.Shared.Maps;
using Content.Shared.Popups;
using Content.Shared.Tag;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Map.Components;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using static Content.Shared.Physics.CollisionGroup;

namespace Content.Shared._MC.Xeno.Abilities.LayEgg;

public sealed class LayEggSystem : EntitySystem
{
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly SharedMapSystem _map = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly TagSystem _tags = default!;
    [Dependency] private readonly TurfSystem _turf = default!;
    [Dependency] private readonly RMCActionsSystem _rmcActions = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly XenoEggSystem _xenoEgg = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedXenoHiveSystem _xenoHive = default!;
    [Dependency] private readonly SharedXenoWeedsSystem _weeds = default!;

    private static readonly ProtoId<TagPrototype> AirlockTag = "Airlock";
    private static readonly ProtoId<TagPrototype> StructureTag = "Structure";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MCXenoLayEggComponent, MCXenoLayEggActionEvent>(OnAction);
        SubscribeLocalEvent<MCXenoLayEggComponent, MCXenoLayEggDoAfterEvent>(OnDoAfter);
    }

    private void OnAction(Entity<MCXenoLayEggComponent> entity, ref MCXenoLayEggActionEvent args)
    {
        if (args.Handled)
            return;

        if (!CanPlaceEggPopup(entity))
            return;

        if (!_rmcActions.CanUseActionPopup(entity, args.Action, entity))
            return;

        var ev = new MCXenoLayEggDoAfterEvent();
        var doAfter = new DoAfterArgs(EntityManager, entity, entity.Comp.Delay, ev, entity)
        {
            NeedHand = true,
            BreakOnMove = true,
            RequireCanInteract = true,
        };

        _doAfter.TryStartDoAfter(doAfter);
    }

    private void OnDoAfter(Entity<MCXenoLayEggComponent> entity, ref MCXenoLayEggDoAfterEvent args)
    {
        if (args.Handled)
            return;

        if (args.Cancelled)
            return;


        foreach (var action in _rmcActions.GetActionsWithEvent<MCXenoLayEggActionEvent>(entity))
        {
            if (!_rmcActions.TryUseAction(entity, action, entity))
                return;

            _actions.StartUseDelay((action, action));
        }

        _audio.PlayPredicted(entity.Comp.Sound, entity, args.User);

        if (_net.IsClient)
            return;

        var instance = Spawn(entity.Comp.ProtoId, Transform(entity).Coordinates);
        _xenoHive.SetSameHive(instance, entity.Owner);
        _transform.SetLocalRotation(instance, 0);
        _xenoEgg.SetEggState((instance, Comp<XenoEggComponent>(instance)), XenoEggState.Growing);
        _transform.AnchorEntity(instance, Transform(instance));
    }

    private bool CanPlaceEggPopup(Entity<MCXenoLayEggComponent> entity)
    {
        var coordinates = Transform(entity).Coordinates;
        if (_transform.GetGrid(coordinates) is not { } gridId || !TryComp<MapGridComponent>(gridId, out var grid))
            return false;

        var tile = _map.TileIndicesFor(gridId, grid, coordinates);
        var anchored = _map.GetAnchoredEntitiesEnumerator(gridId, grid, tile);

        if (!_weeds.IsOnWeeds((gridId, grid), coordinates))
        {
            _popup.PopupClient(Loc.GetString("cm-xeno-egg-failed-must-hive-weeds"), entity, entity);
            return false;
        }

        while (anchored.MoveNext(out var uid))
        {
            if (HasComp<XenoEggComponent>(uid))
            {
               _popup.PopupClient(Loc.GetString("cm-xeno-egg-failed-already-there"), uid.Value, entity, PopupType.SmallCaution);
               return false;
            }

            if (!HasComp<XenoConstructComponent>(uid) &&
               !_tags.HasAnyTag(uid.Value, StructureTag, AirlockTag) &&
               !HasComp<StrapComponent>(uid) &&
               !HasComp<XenoTunnelComponent>(uid))
               continue;

            _popup.PopupClient(Loc.GetString("cm-xeno-egg-blocked"), uid.Value, entity, PopupType.SmallCaution);
            return false;
        }

        if (!_turf.IsTileBlocked(gridId, tile, Impassable | MidImpassable | HighImpassable, grid))
            return true;

        _popup.PopupClient(Loc.GetString("cm-xeno-egg-blocked"), coordinates, entity, PopupType.SmallCaution);
        return false;
    }
}
