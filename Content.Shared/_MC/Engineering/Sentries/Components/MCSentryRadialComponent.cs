using Robust.Shared.GameStates;

namespace Content.Shared._MC.Engineering.Sentries.Components;

/// <summary>
/// Controls the radial detection and engagement mode of a sentry.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MCSentryRadialComponent : Component
{
    /// <summary>
    /// Whether the sentry is currently in radial (360-degree) mode.
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool RadialMode;

    /// <summary>
    /// Local facing direction established when the sentry is deployed.
    /// </summary>
    [DataField, AutoNetworkedField]
    public Angle? DeploymentRotation;

    /// <summary>
    /// The default detection/engagement radius.
    /// </summary>
    [DataField]
    public float Radius = 7f;

    /// <summary>
    /// The reduced radius applied when the sentry is in directional (non-radial) mode.
    /// </summary>
    [DataField]
    public float RadiusReduced = 5f;

    /// <summary>
    /// The maximum allowed angular deviation from the facing direction when not in radial mode.
    /// </summary>
    [DataField]
    public Angle MaxDeviation = Angle.FromDegrees(45);
}
