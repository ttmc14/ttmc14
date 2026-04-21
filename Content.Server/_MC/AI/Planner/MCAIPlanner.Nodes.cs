using Content.Server._MC.AI.Planner.Objects;
using Content.Shared._MC.AI.Modules;

namespace Content.Server._MC.AI.Planner;

public static partial class MCAIPlanner
{
    private static int AddNode(int state, int action, int parent, float g, float h)
    {
        var idx = Nodes.Count;

        Nodes.Add(new Node
        {
            State = state,
            ActionIndex = action,
            Parent = parent,
            G = g,
            H = h,
        });

        return idx;
    }

    private static void ReconstructPlan(
        int nodeIndex,
        List<MCAIActionInternal> actions,
        List<MCAIActionInternal> result)
    {
        result.Clear();

        while (nodeIndex >= 0)
        {
            var node = Nodes[nodeIndex];

            if (node.ActionIndex >= 0)
                result.Add(actions[node.ActionIndex]);

            nodeIndex = node.Parent;
        }

        result.Reverse();
    }
}
