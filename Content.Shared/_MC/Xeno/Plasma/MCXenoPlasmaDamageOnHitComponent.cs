using Content.Shared.FixedPoint;
using Robust.Shared.GameStates;

namespace Content.Shared._MC.Xeno.Plasma;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(fieldDeltas: true)]
public sealed partial class MCXenoPlasmaDamageOnHitComponent : Component
{
    [DataField, AutoNetworkedField]
    public FixedPoint2 Amount;

    [DataField, AutoNetworkedField]
    public float Multiplier;
}
