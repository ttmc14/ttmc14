using System.Numerics;
using Content.Shared._MC.Xeno.Portal;
using Content.Shared._RMC14.Actions;
using Content.Shared._RMC14.Xenonids.Hive;
using Content.Shared.Actions;
using Content.Shared.Mobs;
using Robust.Shared.Network;

namespace Content.Shared._MC.Xeno.Abilities.PortalPlacer;

public sealed class MCXenoPortalPlacerSystem : EntitySystem
{
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly RMCActionsSystem _rmcActions = default!;
    [Dependency] private readonly SharedUserInterfaceSystem _ui = default!;
    [Dependency] private readonly SharedXenoHiveSystem _xenoHive = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MCXenoPortalPlacerComponent, MCXenoPortalPlacerActionEvent>(OnAction);
        SubscribeLocalEvent<MCXenoPortalPlacerComponent, MCXenoChoosePortalBuiMsg>(OnChoose);
        SubscribeLocalEvent<MCXenoPortalPlacerComponent, MobStateChangedEvent>(OnMobStateChanged);
        SubscribeLocalEvent<MCXenoPortalPlacerComponent, ComponentShutdown>(OnShutdown);
    }

    private void OnAction(Entity<MCXenoPortalPlacerComponent> entity, ref MCXenoPortalPlacerActionEvent args)
    {
        if (!_rmcActions.CanUseActionPopup(entity, args.Action))
            return;

        _ui.TryOpenUi(entity.Owner, MCXenoPortalPlacerUI.Key, entity);
    }

    private void OnChoose(Entity<MCXenoPortalPlacerComponent> entity, ref MCXenoChoosePortalBuiMsg args)
    {
        _ui.CloseUi(entity.Owner, MCXenoPortalPlacerUI.Key, entity);

        if (args.Id != entity.Comp.PortalFirst.Id && args.Id != entity.Comp.PortalSecond.Id)
            return;

        foreach (var action in _rmcActions.GetActionsWithEvent<MCXenoPortalPlacerActionEvent>(entity))
        {
            if (!_rmcActions.TryUseAction(entity, action, entity))
                return;

            _actions.StartUseDelay((action, action));
        }

        var coordinates = Transform(entity).Coordinates;
        var coordinatesRounded = Transform(entity).Coordinates.WithPosition((coordinates.Position + entity.Comp.Offset).Floored());

        var (activeData, otherData) = entity.Comp.PortalFirst.Id == args.Id
            ? (entity.Comp.PortalFirst, entity.Comp.PortalSecond)
            : (entity.Comp.PortalSecond, entity.Comp.PortalFirst);

        var (active, other) = entity.Comp.PortalFirst.Id == args.Id
            ? (entity.Comp.PortalFirstEntityUid, entity.Comp.PortalSecondEntityUid)
            : (entity.Comp.PortalSecondEntityUid, entity.Comp.PortalFirstEntityUid);

        if (_net.IsClient)
            return;

        if (active is not null)
            QueueDel(active);

        var instance = Spawn(activeData.Id, coordinatesRounded);

        if (entity.Comp.PortalFirst.Id == args.Id)
            entity.Comp.PortalFirstEntityUid = instance;
        else
            entity.Comp.PortalSecondEntityUid = instance;

        _xenoHive.SetSameHive(entity.Owner,  instance);

        if (other is null)
            return;

        var activePortal = EnsureComp<MCXenoPortalComponent>(instance);
        activePortal.LinkedEntity = other.Value;

        Dirty(instance, activePortal);

        var otherPortal = EnsureComp<MCXenoPortalComponent>(other.Value);
        otherPortal.LinkedEntity = instance;

        Dirty(other.Value, otherPortal);
    }

    private void OnMobStateChanged(Entity<MCXenoPortalPlacerComponent> entity, ref MobStateChangedEvent args)
    {
        if (args.NewMobState != MobState.Dead)
            return;

        DeletePortals(entity);
    }

    private void OnShutdown(Entity<MCXenoPortalPlacerComponent> entity, ref ComponentShutdown args)
    {
        DeletePortals(entity);
    }

    private void DeletePortals(Entity<MCXenoPortalPlacerComponent> entity)
    {
        QueueDel(entity.Comp.PortalFirstEntityUid);
        QueueDel(entity.Comp.PortalSecondEntityUid);

        entity.Comp.PortalFirstEntityUid = null;
        entity.Comp.PortalSecondEntityUid = null;
        Dirty(entity);
    }
}
