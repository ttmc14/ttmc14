using Content.Shared.NodeContainer;
using Robust.Shared.Map.Components;

namespace Content.Server.NodeContainer.Nodes
{
    [DataDefinition]
    public sealed partial class PortPipeNode : PipeNode
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

            foreach (var node in NodeHelpers.GetNodesInTile(nodeQuery, grid, gridIndex))
            {
                if (node is PortablePipeNode)
                    yield return node;
            }

            foreach (var node in base.GetReachableNodes(xform, nodeQuery, xformQuery, grid, entMan))
            {
                yield return node;
            }
        }
    }
}
