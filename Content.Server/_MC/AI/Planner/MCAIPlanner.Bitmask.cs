using System.Numerics;
using Content.Server._MC.AI.Planner.Objects;
using Content.Shared._MC.AI;
using Content.Shared._MC.AI.Modules;

namespace Content.Server._MC.AI.Planner;

public static partial class MCAIPlanner
{
    private static int ToState(Dictionary<string, bool> state)
    {
        var bits = 0;

        foreach (var (k, v) in state)
            if (v && KeyMap.TryGetValue(k, out var i))
                bits |= 1 << i;

        return bits;
    }

    private static void ToCondition(
        Dictionary<string, bool> cond,
        out int mask,
        out int value)
    {
        mask = 0;
        value = 0;

        foreach (var (k, v) in cond)
        {
            if (!KeyMap.TryGetValue(k, out var i))
                continue;

            mask |= 1 << i;

            if (v)
                value |= 1 << i;
        }
    }
}
