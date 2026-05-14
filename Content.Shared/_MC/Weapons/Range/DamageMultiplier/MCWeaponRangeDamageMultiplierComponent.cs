using Robust.Shared.GameStates;

namespace Content.Shared._MC.Weapons.Range.DamageMultiplier;

[RegisterComponent, NetworkedComponent]
public sealed partial class MCWeaponRangeDamageMultiplierComponent : Component
{
    [DataField]
    public float Multiplier = 1.0f;
}
