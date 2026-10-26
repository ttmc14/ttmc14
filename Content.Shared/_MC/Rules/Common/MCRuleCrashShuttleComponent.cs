using Content.Shared.Whitelist;
using Robust.Shared.GameStates;

namespace Content.Shared._MC.Rules.Common;

[RegisterComponent, NetworkedComponent]
public sealed partial class MCRuleCrashShuttleComponent : Component
{
    [DataField]
    public TimeSpan Time = TimeSpan.FromMinutes(10);

    [DataField]
    public EntityWhitelist? PointWhitelist;
}
