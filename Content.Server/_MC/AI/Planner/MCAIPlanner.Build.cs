using Content.Server._MC.AI.Planner.Objects;
using Content.Shared._MC.AI.Modules;

namespace Content.Server._MC.AI.Planner;

public static partial class MCAIPlanner
{
    private static void CompileActions(List<MCAIActionInternal> actions)
    {
        foreach (var a in actions)
        {
            ToCondition(a.Preconditions, out var pm, out var pv);
            ToCondition(a.Effects, out var em, out var ev);

            Actions.Add(new ActionData
            {
                PrecMask = pm,
                PrecValue = pv,
                EffMask = em,
                EffValue = ev,
                Cost = a.Cost,
            });
        }
    }

    private static void BuildKeyMap(
        Dictionary<string, bool> start,
        Dictionary<string, bool> goal,
        List<MCAIActionInternal> actions)
    {
        foreach (var k in start.Keys)
            TryAddKey(k);

        foreach (var k in goal.Keys)
            TryAddKey(k);

        foreach (var a in actions)
        {
            foreach (var k in a.Preconditions.Keys)
                TryAddKey(k);

            foreach (var k in a.Effects.Keys)
                TryAddKey(k);
        }
    }

    private static void TryAddKey(string key)
    {
        if (!KeyMap.ContainsKey(key))
            KeyMap[key] = KeyMap.Count;
    }
}
