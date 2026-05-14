using Robust.Shared.GameStates;

namespace Content.Shared._MC.Xeno.Abilities.Widow.SummonAbsorb;

[RegisterComponent, NetworkedComponent]
public sealed partial class MCXenoSummonAbsorbComponent : Component
{
    [DataField]
    public float Scale = 1f;
}
