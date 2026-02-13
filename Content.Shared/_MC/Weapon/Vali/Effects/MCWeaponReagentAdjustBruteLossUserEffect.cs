using Content.Shared._MC.Damage;
using Content.Shared.Damage;
using Content.Shared.FixedPoint;

namespace Content.Shared._MC.Weapon.Vali.Effects;

public sealed partial class MCWeaponReagentAdjustBruteLossUserEffect : MCWeaponReagentEffect
{
    [DataField]
    public float Amount;

    public override void Apply(EntityUid target, EntityUid user, FixedPoint2 damageAmount, DamageSpecifier damageSpecifier, EntityManager entityManager)
    {
        var damageable = entityManager.System<MCDamageableSystem>();
        damageable.AdjustBruteLoss(user, Amount);
    }
}
