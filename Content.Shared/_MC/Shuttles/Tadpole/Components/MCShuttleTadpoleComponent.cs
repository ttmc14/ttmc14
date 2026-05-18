using Content.Shared._MC.Shuttles.Space;
using Content.Shared._MC.Shuttles.Space.Components;
using Content.Shared._MC.Shuttles.TargetPoint;
using Content.Shared._MC.Shuttles.TargetPoint.Components;
using Robust.Shared.GameStates;

namespace Content.Shared._MC.Shuttles.Tadpole.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class MCShuttleTadpoleComponent : Component
{
    #region Spaces

    /// <seealso cref="MCShuttleSpaceSystem"/>
    /// <seealso cref="MCShuttleSpaceComponent"/>
    [DataField]
    public string SpaceFly = "mc-tadpole-fly";

    /// <seealso cref="MCShuttleSpaceSystem"/>
    /// <seealso cref="MCShuttleSpaceComponent"/>
    [DataField]
    public string SpaceOrbit = "mc-tadpole-orbit";

    #endregion

    /// <seealso cref="MCShuttleTargetPointComponent"/>
    /// <seealso cref="MCShuttleTargetPointSystem"/>
    [DataField]
    public string TargetPointReturn = "mc-tadpole-return";
}
