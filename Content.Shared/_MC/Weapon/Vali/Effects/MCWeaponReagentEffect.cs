using Content.Shared.Damage;
using Content.Shared.FixedPoint;
using JetBrains.Annotations;

namespace Content.Shared._MC.Weapon.Vali.Effects;

[ImplicitDataDefinitionForInheritors, MeansImplicitUse]
public abstract partial class MCWeaponReagentEffect
{
    public abstract void Apply(EntityUid target, EntityUid user, FixedPoint2 damageAmount, DamageSpecifier damageSpecifier, EntityManager entityManager);
}
