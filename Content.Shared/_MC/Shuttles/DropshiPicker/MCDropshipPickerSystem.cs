using System.Linq;
using System.Numerics;
using Content.Shared._MC.Shuttles.DropshiPicker.Components;
using Content.Shared._MC.Shuttles.DropshiPicker.UI;
using Content.Shared._MC.Shuttles.FTL;
using Content.Shared._MC.Shuttles.Space;
using Content.Shared._MC.Shuttles.TargetPoint;
using Robust.Shared.EntitySerialization.Systems;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Utility;

namespace Content.Shared._MC.Shuttles.DropshiPicker;

public sealed class MCDropshipPickerSystem : EntitySystem
{
    [Dependency] private readonly MapLoaderSystem _mapLoader = null!;
    [Dependency] private readonly SharedTransformSystem _transform = null!;
    [Dependency] private readonly SharedJointSystem _jointSystem = null!;

    [Dependency] private readonly MCShuttleFTLSharedSystem _mcFTL = null!;
    [Dependency] private readonly MCShuttleSpaceSystem _mcSpace = null!;
    [Dependency] private readonly MCShuttleTargetPointSystem _mcTargetPoint = null!;

    private const string SpawnerJointId = "shuttle_spawn_weld_joint";

    public override void Initialize()
    {
        Subs.BuiEvents<MCDropshipPickerComponent>(MCDropshipPickerUI.Key,
            sub =>
            {
                sub.Event<MCDropshipPickerSelectBuiMessage>(OnDropshipSelect);
            }
        );
    }

    private void OnDropshipSelect(Entity<MCDropshipPickerComponent> entity, ref MCDropshipPickerSelectBuiMessage args)
    {
        var path = new ResPath(args.Path);

        if (!entity.Comp.DropshipGrids.Select(e => e.Path).Contains(path))
        {
            Log.Warning($"Dropship selection by {ToPrettyString(args.Actor)}, don't represent in picker {path}");
            return;
        }

        if (!_mcTargetPoint.TryGetPointCoordinates(entity.Comp.LandPoint, out var coordinates, out var pointEntity))
        {
            Log.Warning($"Dropship landpoint not found: {entity.Comp.LandPoint}");
            return;
        }

        _mcSpace.EnsureMap(entity.Comp.SpaceCreation, out var mapId, out _);

        if (!_mapLoader.TryLoadGrid(mapId, path, out var grid))
        {
            Log.Warning($"Dropship grid failed to load: {path}");
            return;
        }

        if (entity.Comp.FTL)
        {
            _mcFTL.FTLToCoordinates(grid.Value, _transform.ToCoordinates(coordinates), Angle.Zero, 0.1f, 0.1f);
            return;
        }

        _transform.SetMapCoordinates(grid.Value, coordinates);

        WeldShuttleToParent(grid.Value, pointEntity, coordinates);
    }

    private void WeldShuttleToParent(EntityUid spawnedShuttle, EntityUid pointEntity, MapCoordinates coordinates)
    {
        var targetGridUid = Transform(pointEntity).GridUid;
        if (targetGridUid == null || !HasComp<MapGridComponent>(targetGridUid.Value))
            return;

        var parentGrid = targetGridUid.Value;

        if (!TryComp<PhysicsComponent>(spawnedShuttle, out var physicsChild) ||
            !TryComp<PhysicsComponent>(parentGrid, out var physicsParent))
            return;

        SharedJointSystem.LinearStiffness(
            2f,
            0.7f,
            physicsChild.Mass,
            physicsParent.Mass,
            out var stiffness,
            out var damping);

        var joint = _jointSystem.GetOrCreateWeldJoint(spawnedShuttle, parentGrid, SpawnerJointId + spawnedShuttle);

        var childXform = Transform(spawnedShuttle);
        var parentXform = Transform(parentGrid);

        joint.LocalAnchorA = Vector2.Transform(coordinates.Position, _transform.GetInvWorldMatrix(childXform));
        joint.LocalAnchorB = Vector2.Transform(coordinates.Position, _transform.GetInvWorldMatrix(parentXform));

        joint.ReferenceAngle = (float) (_transform.GetWorldRotation(parentXform) - _transform.GetWorldRotation(childXform));
        joint.CollideConnected = false;

        joint.Stiffness = stiffness;
        joint.Damping = damping;

        Log.Debug($"Successfully welded independent shuttle {spawnedShuttle} to target shuttle {parentGrid}");
    }
}
