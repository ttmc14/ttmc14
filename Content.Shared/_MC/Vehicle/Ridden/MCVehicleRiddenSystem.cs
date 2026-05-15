using Content.Shared._MC.Vehicle.Operated;
using Content.Shared._MC.Vehicle.Operated.Events;
using Content.Shared._MC.Vehicle.Ridden.Components;
using Content.Shared.ActionBlocker;
using Content.Shared.Examine;
using Content.Shared.Interaction;
using Content.Shared.Interaction.Components;
using Content.Shared.Inventory.VirtualItem;
using Content.Shared.Movement.Events;

namespace Content.Shared._MC.Vehicle.Ridden;

public sealed partial class MCVehicleRiddenSystem : EntitySystem
{
    [Dependency] private readonly ActionBlockerSystem _actionBlocker = null!;
    [Dependency] private readonly SharedTransformSystem _transform = null!;
    [Dependency] private readonly SharedVirtualItemSystem _virtualItem = null!;
    [Dependency] private readonly MCVehicleOperatedSystem _vehicleOperated = null!;

    public override void Initialize()
    {
        SubscribeLocalEvent<MCVehicleRiddenComponent, MCVehicleOperatedChangedEvent>(OnOperatedChanged);
        SubscribeLocalEvent<MCVehicleRiddenComponent, ComponentShutdown>(OnShutdown);

        SubscribeLocalEvent<MCVehicleRiddenComponent, ExaminedEvent>(OnExamined);
        SubscribeLocalEvent<MCVehicleRiddenComponent, InteractUsingEvent>(OnInteractUsing);
        SubscribeLocalEvent<MCVehicleRiddenComponent, UpdateCanMoveEvent>(OnCanMove);
    }

    private void OnExamined(Entity<MCVehicleRiddenComponent> entity, ref ExaminedEvent args)
    {
        if (!args.IsInDetailsRange)
            return;

        var fuelPercent = entity.Comp.Fuel / entity.Comp.FuelMax * 100f;

        args.PushText(Loc.GetString(
            "mc-vehicle-fuel-examine",
            ("fuel", fuelPercent.ToString("0.0"))));
    }

    private void OnOperatedChanged(
        Entity<MCVehicleRiddenComponent> entity,
        ref MCVehicleOperatedChangedEvent args)
    {
        ref var comp = ref entity.Comp;

        comp.LastPosition = _transform.GetWorldPosition(entity);
        comp.Operated = args.NewOperator != null;

        DirtyFields(
            entity,
            comp,
            null,
            nameof(MCVehicleRiddenComponent.LastPosition),
            nameof(MCVehicleRiddenComponent.Operated));

        if (args.NewOperator is { } newOperator)
            AddVirtualHands(entity, newOperator);

        if (args.OldOperator is { } oldOperator)
            RemoveVirtualHands(entity, oldOperator);
    }

    private void OnShutdown(Entity<MCVehicleRiddenComponent> entity, ref ComponentShutdown args)
    {
        if (!_vehicleOperated.TryGetOperator(entity.Owner, out var operatorEntity))
            return;

        RemoveVirtualHands(entity, operatorEntity);
    }

    private void OnCanMove(Entity<MCVehicleRiddenComponent> entity, ref UpdateCanMoveEvent args)
    {
        if (entity.Comp.Fuel <= 0)
            args.Cancel();
    }

    public override void Update(float frameTime)
    {
        var query = EntityQueryEnumerator<MCVehicleRiddenComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (!comp.Operated)
                continue;

            if (comp.Fuel <= 0)
                continue;

            ConsumeFuel(uid, comp);
        }
    }

    private void ConsumeFuel(EntityUid uid, MCVehicleRiddenComponent comp)
    {
        var currentPosition = _transform.GetWorldPosition(uid);
        var distance = (currentPosition - comp.LastPosition).Length();

        comp.LastPosition = currentPosition;
        DirtyField(uid, comp, nameof(MCVehicleRiddenComponent.LastPosition));

        if (distance <= 0f)
            return;

        var fuelCost = distance * comp.FuelCost;
        if (fuelCost <= 0f)
            return;

        comp.Fuel = float.Max(0f, comp.Fuel - fuelCost);

        if (comp.Fuel == 0f)
            _actionBlocker.UpdateCanMove(uid);

        DirtyField(uid, comp, nameof(MCVehicleRiddenComponent.Fuel));
    }

    private void AddVirtualHands(
        Entity<MCVehicleRiddenComponent> vehicle,
        EntityUid operatorUid)
    {
        for (var i = 0; i < vehicle.Comp.HandsRequired; i++)
        {
            _virtualItem.TrySpawnVirtualItemInHand(
                vehicle,
                operatorUid,
                out var virtualItem,
                true);

            if (virtualItem is not { } item)
                continue;

            EnsureComp<UnremoveableComponent>(item);
        }
    }

    private void RemoveVirtualHands(
        Entity<MCVehicleRiddenComponent> vehicle,
        EntityUid operatorUid)
    {
        _virtualItem.DeleteInHandsMatching(operatorUid, vehicle);
    }
}
