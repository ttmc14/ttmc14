using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._MC.Xeno.Abilities.Runner.Pounce.Trail;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(fieldDeltas: true)]
public sealed partial class MCXenoPounceTrailComponent : Component
{
    [DataField, AutoNetworkedField]
    public EntProtoId TrailId;

    [AutoNetworkedField]
    public Vector2i? LastTurf;
}
