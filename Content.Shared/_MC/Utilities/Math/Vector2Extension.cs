using System.Numerics;

namespace Content.Shared._MC.Utilities.Math;

public static class Vector2Extension
{
    public static Vector2 CardinalDirection(this Vector2 vector)
    {
        return MathF.Abs(vector.X) > MathF.Abs(vector.Y)
            ? MathF.Sign(vector.X) * Vector2.UnitX
            : MathF.Sign(vector.Y) * Vector2.UnitY;
    }
}
