using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Shared._MC.Map;

[RegisterComponent, NetworkedComponent]
public sealed partial class MCPlanetMapPrototypeComponent : Component
{
    [DataField]
    public List<ResPath> MapsBelow = new();

    [DataField]
    public List<ResPath> MapsAbove = new();

    [DataField]
    public ComponentRegistry ZLevelsComponentOverrides = new();
}
