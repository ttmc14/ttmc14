using System.Numerics;

namespace Content.Shared._MC.ZLevels.Weapons.Components;

[RegisterComponent]
public sealed partial class MCZLevelPredictedProjectileVisualOffsetComponent : Component
{
    public Vector2 Offset;
    public Vector2? OriginalOffset;
    public Vector2 AppliedOffset;
}
