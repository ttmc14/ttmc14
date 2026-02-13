using Content.Shared.Damage;
using Content.Shared.FixedPoint;

namespace Content.Shared._MC.Weapon.Vali.Effects;

public sealed partial class MCWeaponReagentDamageMultipliedEffect : MCWeaponReagentEffect
{
    [DataField]
    public float Multiplier = 1f;

    [DataField]
    public bool IgnoreResistance = true;

    public override void Apply(EntityUid target, EntityUid user, FixedPoint2 damageAmount, DamageSpecifier damageSpecifier, EntityManager entityManager)
    {
        var damageSys = entityManager.System<DamageableSystem>();
        damageSys.TryChangeDamage(target, damageSpecifier * FixedPoint2.New(Multiplier), IgnoreResistance, origin: user);
    }
}
