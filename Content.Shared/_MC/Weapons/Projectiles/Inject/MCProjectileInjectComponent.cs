using Content.Shared.Chemistry.Reagent;
using Robust.Shared.GameStates;

namespace Content.Shared._MC.Weapons.Projectiles.Inject;

[RegisterComponent, NetworkedComponent]
public sealed partial class MCProjectileInjectComponent : Component
{
    [DataField]
    public string Solution = "chemicals";

    [DataField]
    public List<ReagentQuantity> Reagents = new();

    [DataField]
    public bool IgnoreArmor;

    #region Effects

    [DataField]
    public bool Effect = true;

    [DataField]
    public Color EffectColor = Color.DarkOrange;

    #endregion
}
