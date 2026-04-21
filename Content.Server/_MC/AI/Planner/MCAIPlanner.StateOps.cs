using Content.Server._MC.AI.Planner.Objects;

namespace Content.Server._MC.AI.Planner;

public static partial class MCAIPlanner
{
    private static bool CheckPreconditions(int state, ActionData action)
    {
        return (state & action.PrecMask) == action.PrecValue;
    }

    private static int ApplyEffects(int state, ActionData action)
    {
        return (state & ~action.EffMask) | action.EffValue;
    }

    private static bool IsGoal(int state, int mask, int value)
    {
        return (state & mask) == value;
    }
}
