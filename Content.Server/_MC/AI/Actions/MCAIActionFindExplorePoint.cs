using System.Numerics;
using Content.Shared._MC.AI;
using Content.Shared._MC.AI.Modules;
using Robust.Shared.Map;
using Robust.Shared.Random;

namespace Content.Server._MC.AI.Actions;

public sealed partial class MCAIActionFindExplorePoint : MCAIAction<MCAIActionFindExplorePoint>
{
    [DataField]
    public float ExploreRadius = 8f;

    [DataField]
    public int SampleDirections = 12;

    [DataField(required: true)]
    public string OutputTargetKey = string.Empty;
}

public sealed partial class MCAIActionFindExplorePointSystem : MCAIActionSystem<MCAIActionFindExplorePoint>
{
    [Dependency] private readonly IMapManager _mapManager = null!;
    [Dependency] private readonly SharedMapSystem _mapSystem = null!;
    [Dependency] private readonly SharedTransformSystem _transform = null!;
    [Dependency] private readonly IRobustRandom _random = null!;

    protected override MCAIActionStatus OnActionUpdate(Entity<MCAIAgentComponent> entity, MCAIActionFindExplorePoint action, float frameTime)
    {
        var destination = PickDestination(Transform(entity), action);
        if (destination is null)
            return MCAIActionStatus.Failed;

        var transform = Transform(entity);
        var invMatrix = _transform.GetInvWorldMatrix(transform.ParentUid);
        var localPos = Vector2.Transform(destination.Value, invMatrix);
        var coords = new EntityCoordinates(transform.ParentUid, localPos);

        entity.Comp.Memory.ContainerSet(action.OutputTargetKey, coords);

        return MCAIActionStatus.Finished;
    }

    private Vector2? PickDestination(TransformComponent transform, MCAIActionFindExplorePoint action)
    {
        var worldPos = _transform.GetWorldPosition(transform);
        var mapId = transform.MapID;

        float[] distances =
        [
            action.ExploreRadius,
            action.ExploreRadius * 0.65f,
            action.ExploreRadius * 0.4f,
            action.ExploreRadius * 0.2f,
            2f,
        ];

        var baseAngle = (float) _random.NextAngle().Theta;

        foreach (var dist in distances)
        {
            if (dist < 1f)
                continue;

            var angleStep = float.Pi * 2f / action.SampleDirections;
            for (var i = 0; i < action.SampleDirections; i++)
            {
                var angle = baseAngle + angleStep * i;
                var dir = new Vector2(MathF.Cos(angle), MathF.Sin(angle));
                var candidatePos = worldPos + dir * dist;

                var mapCoords = new MapCoordinates(candidatePos, mapId);
                if (!_mapManager.TryFindGridAt(mapCoords, out var gridUid, out var grid))
                    continue;

                var tileIndices = _mapSystem.WorldToTile(gridUid, grid, candidatePos);

                if (!_mapSystem.TryGetTileRef(gridUid, grid, tileIndices, out var tileRef) ||
                    tileRef.Tile.IsEmpty)
                    continue;

                if (_mapSystem.AnchoredEntityCount(gridUid, grid, tileIndices) > 0)
                    continue;

                return candidatePos;
            }
        }

        return null;
    }
}
