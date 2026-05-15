using Content.Shared._MC.Vehicle.Operated.Components;
using Content.Shared.Buckle.Components;

namespace Content.Shared._MC.Vehicle.Operated;

public sealed partial class MCVehicleOperatedSystem
{
    private void InitializeEnter()
    {
        SubscribeLocalEvent<MCVehicleOperatedStrapComponent, StrappedEvent>(OnEnterVehicleStrapped);
        SubscribeLocalEvent<MCVehicleOperatedStrapComponent, UnstrappedEvent>(OnEnterVehicleUnstrapped);
    }

    private void OnEnterVehicleStrapped(Entity<MCVehicleOperatedStrapComponent> entity, ref StrappedEvent args)
    {
        if (!TryComp<MCVehicleOperatedComponent>(entity, out var vehicle))
            return;

        TrySetOperator((entity, vehicle), args.Buckle);
    }

    private void OnEnterVehicleUnstrapped(Entity<MCVehicleOperatedStrapComponent> entity, ref UnstrappedEvent args)
    {
        if (!TryComp<MCVehicleOperatedComponent>(entity, out var vehicle))
            return;

        TrySetOperator((entity, vehicle), null);
    }
}
