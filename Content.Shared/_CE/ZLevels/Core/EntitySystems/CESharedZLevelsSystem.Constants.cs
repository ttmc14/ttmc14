namespace Content.Shared._CE.ZLevels.Core.EntitySystems;

public abstract partial class CESharedZLevelsSystem
{
    public const int MaxZLevelsBelowRendering = 3;
    public static float ZLevelOffset = 0f;

    private const float ZGravityForce = 9.8f;
    private const float ZVelocityLimit = 20.0f;
    private const int MaxStepsPerFrame = 10;

    /// <summary>
    /// The minimum speed required to trigger LandEvent events.
    /// </summary>
    private const float ImpactVelocityLimit = 3.5f;
}
