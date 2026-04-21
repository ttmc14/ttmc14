using System.Numerics;
using Content.Server._MC.AI.Planner.Objects;
using Content.Shared._MC.AI;
using Content.Shared._MC.AI.Modules;

namespace Content.Server._MC.AI.Planner;

public static partial class MCAIPlanner
{
    private static readonly Dictionary<string, int> KeyMap = new();
    private static readonly List<ActionData> Actions = new();
    private static readonly List<Node> Nodes = new();
    private static readonly PriorityQueue<int, float> Open = new();
    private static readonly Dictionary<int, float> BestG = new();

    public static bool Plan(
        MCAIMemory memory,
        Dictionary<string, bool> goalState,
        List<MCAIActionInternal> availableActions,
        List<MCAIActionInternal> resultPlan,
        int maxIterations = 256)
    {
        Reset();

        var startStateDict = memory.StateCopy();

        BuildKeyMap(startStateDict, goalState, availableActions);

        var start = ToState(startStateDict);
        ToCondition(goalState, out var goalMask, out var goalValue);

        CompileActions(availableActions);

        return RunAStar(start, goalMask, goalValue, availableActions, resultPlan, maxIterations);
    }

    private static bool RunAStar(
        int startState,
        int goalMask,
        int goalValue,
        List<MCAIActionInternal> rawActions,
        List<MCAIActionInternal> outPlan,
        int maxIterations)
    {
        var startH = Heuristic(startState, goalMask, goalValue);
        var startIdx = AddNode(startState, -1, -1, 0f, startH);

        Open.Enqueue(startIdx, startH);
        BestG[startState] = 0f;

        var iterations = 0;

        while (Open.Count > 0 && iterations++ < maxIterations)
        {
            var currentIdx = Open.Dequeue();
            var current = Nodes[currentIdx];

            if (IsGoal(current.State, goalMask, goalValue))
            {
                ReconstructPlan(currentIdx, rawActions, outPlan);
                return true;
            }

            ExpandNode(currentIdx, goalMask, goalValue);
        }

        return false;
    }

    private static void ExpandNode(int nodeIndex, int goalMask, int goalValue)
    {
        var node = Nodes[nodeIndex];

        for (var i = 0; i < Actions.Count; i++)
        {
            var action = Actions[i];

            if (!CheckPreconditions(node.State, action))
                continue;

            var newState = ApplyEffects(node.State, action);
            var newG = node.G + action.Cost;

            if (BestG.TryGetValue(newState, out var best) && best <= newG)
                continue;

            BestG[newState] = newG;

            var h = Heuristic(newState, goalMask, goalValue);
            var newIdx = AddNode(newState, i, nodeIndex, newG, h);

            Open.Enqueue(newIdx, newG + h);
        }
    }

    private static float Heuristic(int state, int goalMask, int goalValue)
    {
        var diff = (state ^ goalValue) & goalMask;
        return BitOperations.PopCount((uint)diff);
    }

    private static void Reset()
    {
        KeyMap.Clear();
        Actions.Clear();
        Nodes.Clear();
        Open.Clear();
        BestG.Clear();
    }
}
