using Content.Shared._MC.Vehicle.Operated.Components;

namespace Content.Shared._MC.Vehicle.Operated;

public sealed partial class MCVehicleOperatedSystem
{
    private void InitializeOperator()
    {
        SubscribeLocalEvent<MCVehicleOperatorComponent, ComponentShutdown>(OnOperatorShutdown);
    }

    private void OnOperatorShutdown(Entity<MCVehicleOperatorComponent> entity, ref ComponentShutdown args)
    {
        TryRemoveOperator((entity, entity));
    }
}
