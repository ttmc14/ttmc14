using Content.Server.NodeContainer.Nodes;
using Content.Shared.NodeContainer;
using Robust.Shared.Map.Components;

namespace Content.Server.Power.Nodes
{
    [DataDefinition]
    public sealed partial class CableTerminalNode : CableDeviceNode
    {
        [Dependency] private readonly SharedMapSystem _mapSystem = default!;
        public override IEnumerable<Node> GetReachableNodes(TransformComponent xform,
            EntityQuery<NodeContainerComponent> nodeQuery,
            EntityQuery<TransformComponent> xformQuery,
            MapGridComponent? grid,
            IEntityManager entMan)
        {
            if (!xform.Anchored || grid == null)
                yield break;

            var gridEntity = new Entity<MapGridComponent>(xform.GridUid ?? EntityUid.Invalid, grid);
            var gridIndex = _mapSystem.TileIndicesFor(gridEntity, xform.Coordinates);

            var dir = xform.LocalRotation.GetDir();
            var targetIdx = gridIndex.Offset(dir);

            foreach (var node in NodeHelpers.GetNodesInTile(nodeQuery, grid, targetIdx))
            {
                if (node is CableTerminalPortNode)
                    yield return node;
            }

            foreach (var node in base.GetReachableNodes(xform, nodeQuery, xformQuery, grid, entMan))
            {
                yield return node;
            }
        }
    }
}
