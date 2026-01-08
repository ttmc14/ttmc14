using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._MC.Xeno.Abilities.Runner.Pounce.Effect;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(fieldDeltas: true)]
public sealed partial class MCXenoPounceEffectComponent : Component
{
    [DataField, AutoNetworkedField]
    public EntProtoId EntityId;
}
