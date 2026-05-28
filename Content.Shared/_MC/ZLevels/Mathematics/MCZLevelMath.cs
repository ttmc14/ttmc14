using System.Numerics;
using Content.Shared._CE.ZLevels.Core.EntitySystems;
using JetBrains.Annotations;

namespace Content.Shared._MC.ZLevels.Mathematics ;

public static class MCZLevelMath
{
    [PublicAPI] public const float CrossZShotRange = 4f;
    [PublicAPI] public const float CrossZOpeningSourceNudge = 0.30f;
    [PublicAPI] public const float CrossZOpeningSourceEdgeRangeTiles = 2f;

    [PublicAPI]
    public static Vector2 ClampCrossZShotTarget(Vector2 from, Vector2 to)
    {
        var delta = to - from;
        var distance = delta.Length();

        if (distance <= CrossZShotRange || distance <= 0.001f)
            return to;

        return from + delta / distance * CrossZShotRange;
    }

    [PublicAPI]
    public static void GetCrossZProjectilePath(
        Vector2 from,
        Vector2 to,
        Vector2 clampedTo,
        Vector2 opening,
        int offset,
        out Vector2 projectileFrom,
        out Vector2 projectileTo)
    {
        projectileFrom = NudgeOpeningTowardSource(opening, from);

        var direction = to - from;
        if (direction.LengthSquared() <= 0.001f)
            direction = clampedTo - projectileFrom;

        if (direction.LengthSquared() <= 0.001f)
        {
            projectileTo = clampedTo;
            return;
        }

        var distance = float.Max(1f, Vector2.Distance(projectileFrom, clampedTo));

        projectileTo = projectileFrom + Vector2.Normalize(direction) * distance;
    }

    [PublicAPI]
    public static Vector2 NudgeOpeningTowardSource(Vector2 opening, Vector2 source)
    {
        var sourceDirection = source - opening;
        if (sourceDirection.LengthSquared() <= 0.001f)
            return opening;

        return opening + Vector2.Normalize(sourceDirection) * CrossZOpeningSourceNudge;
    }

    [PublicAPI]
    public static Vector2 GetCrossZRenderOffset(int offset)
    {
        return new Vector2(0f, CESharedZLevelsSystem.ZLevelOffset * offset);
    }
}
