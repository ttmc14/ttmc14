using Content.Shared._MC.Vehicle.Grid.Components;
using Content.Shared._MC.Vehicle.Operated;
using Content.Shared.Interaction;
using Robust.Shared.EntitySerialization.Systems;
using Robust.Shared.Network;

namespace Content.Shared._MC.Vehicle.Grid;

public sealed partial class MCVehicleSystem : EntitySystem
{
    [Dependency] private readonly INetManager _net = null!;

    [Dependency] private readonly SharedEyeSystem _eye = null!;
    [Dependency] private readonly SharedMapSystem _map = null!;
    [Dependency] private readonly MapLoaderSystem _mapLoader = null!;
    [Dependency] private readonly SharedTransformSystem _transform = null!;

    [Dependency] private readonly MCVehicleOperatedSystem _mcVehicleOperated = null!;

    public override void Initialize()
    {
        base.Initialize();

        InitializeSeat();

        SubscribeLocalEvent<MCVehicleComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<MCVehicleComponent, ActivateInWorldEvent>(OnWorldInteract);

        SubscribeLocalEvent<MCVehicleGridExitPointComponent, ActivateInWorldEvent>(OnExitWorldInteract);
    }

    private void OnMapInit(Entity<MCVehicleComponent> entity, ref MapInitEvent _)
    {
        LoadMap(entity);
    }

    private void OnWorldInteract(Entity<MCVehicleComponent> entity, ref ActivateInWorldEvent args)
    {
        TryEnter(entity, args.User);
    }

    private void OnExitWorldInteract(Entity<MCVehicleGridExitPointComponent> entity, ref ActivateInWorldEvent args)
    {
        if (!TryGetVehicle(entity, out var vehicle))
            return;

        TryLeave(vehicle, args.User, entity.Comp.Direction);
    }

    private bool TryGetVehicle(EntityUid uid, out Entity<MCVehicleComponent> entity)
    {
        entity = default;

        var gridUid = _transform.GetGrid(uid);
        if (!TryComp<MCVehicleGridComponent>(gridUid, out var gridComponent))
            return false;

        if (gridComponent.OwnerUid is not { } vehicleUid)
            return false;

        if (!TryComp<MCVehicleComponent>(vehicleUid, out var vehicleComponent))
            return false;

        entity = (vehicleUid, vehicleComponent);
        return true;
    }
}
