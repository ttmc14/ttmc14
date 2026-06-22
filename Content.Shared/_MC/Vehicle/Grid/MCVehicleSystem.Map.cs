using System.Numerics;
using Content.Shared._MC.Vehicle.Grid.Components;
using Robust.Shared.EntitySerialization.Systems;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;

namespace Content.Shared._MC.Vehicle.Grid;

public sealed partial class MCVehicleSystem
{
    private const float GridOffsetStep = 10f;

    [Dependency] private readonly MetaDataSystem _meta = null!;

    private void EnsureMap(out MapId id, out Entity<MCVehicleMapComponent> entity)
    {
        var query = EntityQueryEnumerator<MCVehicleMapComponent, MapComponent>();
        while (query.MoveNext(out var uid, out var component, out var mapComponent))
        {
            id = mapComponent.MapId;
            entity = (uid, component);
            return;
        }

        var newUid = _map.CreateMap(out var mapId);
        var newComponent = EnsureComp<MCVehicleMapComponent>(newUid);

        _meta.SetEntityName(newUid, "Vehicle");

        id = mapId;
        entity = (newUid, newComponent);
    }

    private void LoadMap(Entity<MCVehicleComponent> entity)
    {
        // Content.Server ha ha
        if (_net.IsClient)
            return;

        EnsureMap(out var mapId, out var mapEntity);

        if (!_mapLoader.TryLoadGrid(mapId, entity.Comp.Path, out var grid))
        {
            Log.Error($"Failed load {entity.Comp.Path} from {ToPrettyString(entity)}");
            return;
        }

        _meta.SetEntityName(grid.Value, $"Vehicle {ToPrettyString(entity)}");
        _transform.SetWorldPosition(grid.Value, mapEntity.Comp.Offset);

        entity.Comp.GridUid = grid.Value;
        DirtyField(entity.Owner, entity.Comp, nameof(MCVehicleComponent.GridUid));

        mapEntity.Comp.Offset += Vector2.UnitX * GridOffsetStep;
        DirtyField(mapEntity.Owner, mapEntity.Comp, nameof(MCVehicleMapComponent.Offset));

        var gridComponent = EnsureComp<MCVehicleGridComponent>(grid.Value);
        gridComponent.OwnerUid = entity;

        DirtyField(grid.Value, gridComponent, nameof(MCVehicleGridComponent.OwnerUid));
    }
}
