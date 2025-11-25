using Content.Shared.FixedPoint;
using Robust.Shared.GameStates;

namespace Content.Shared._MC.Xeno.Evolution;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MCXenoEvolutionAffectGainComponent : Component
{
    [DataField, AutoNetworkedField]
    public FixedPoint2 Multiplier = 1;

    [DataField, AutoNetworkedField]
    public FixedPoint2 Additional = FixedPoint2.Zero;
}
