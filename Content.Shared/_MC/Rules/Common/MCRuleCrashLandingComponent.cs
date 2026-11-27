namespace Content.Shared._MC.Rules.Common;

[RegisterComponent]
public sealed partial class MCRuleCrashLandingComponent : Component
{
    public EntityUid? Shuttle;
    public bool Landed;
}

[ByRefEvent]
public readonly record struct MCCrashLandedEvent;
