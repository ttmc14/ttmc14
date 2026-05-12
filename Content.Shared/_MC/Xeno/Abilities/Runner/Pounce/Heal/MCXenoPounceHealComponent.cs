using Robust.Shared.GameStates;

namespace Content.Shared._MC.Xeno.Abilities.Runner.Pounce.Heal;

[RegisterComponent, NetworkedComponent]
public sealed partial class MCXenoPounceHealComponent : Component
{
    [DataField]
    public float AdjustPlasma = 35f;

    [DataField]
    public float AdjustHealth = 25f;
}
