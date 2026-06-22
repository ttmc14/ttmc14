using Content.Shared._MC.Vehicle.Grid.Components;
using Content.Shared.Buckle.Components;
using Content.Shared.Interaction;
using Robust.Shared.Map;
using Robust.Shared.Network;

namespace Content.Shared._MC.Vehicle.Grid;

public sealed partial class MCVehicleSystem
{
    private void InitializeSeat()
    {
        SubscribeLocalEvent<MCVehicleGridDriverSeatComponent, StrappedEvent>(OnDriverSeatStrapped);
        SubscribeLocalEvent<MCVehicleGridDriverSeatComponent, UnstrappedEvent>(OnDriverSeatUnstrapped);
    }

    private void OnDriverSeatStrapped(Entity<MCVehicleGridDriverSeatComponent> entity, ref StrappedEvent args)
    {
        // NOTE: content server?
        if (_net.IsClient)
            return;

        if (!TryGetVehicle(entity, out var vehicle))
            return;

        TrySetOperator(vehicle, args.Buckle.Owner);
    }

    private void OnDriverSeatUnstrapped(Entity<MCVehicleGridDriverSeatComponent> entity, ref UnstrappedEvent args)
    {
        // NOTE: content server?
        if (_net.IsClient)
            return;

        if (!TryGetVehicle(entity, out var vehicle))
            return;

        TryRemoveOperator(vehicle, args.Buckle.Owner);
    }

    private void TrySetOperator(Entity<MCVehicleComponent> entity, EntityUid owner)
    {
        _mcVehicleOperated.TrySetOperator(entity.Owner, owner);
        _eye.SetTarget(owner, entity);
    }

    private void TryRemoveOperator(Entity<MCVehicleComponent> entity, EntityUid owner)
    {
        _mcVehicleOperated.TryRemoveOperated(entity.Owner);
        _eye.SetTarget(owner, null);
    }
}
