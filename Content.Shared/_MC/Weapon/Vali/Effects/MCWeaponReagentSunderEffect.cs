using Content.Shared._MC.Xeno.Sunder;
using Content.Shared.Damage;
using Content.Shared.FixedPoint;

namespace Content.Shared._MC.Weapon.Vali.Effects;

public sealed partial class MCWeaponReagentSunderEffect : MCWeaponReagentEffect
{
    [DataField]
    public float Amount;

    public override void Apply(EntityUid target, EntityUid user, FixedPoint2 damageAmount, DamageSpecifier damageSpecifier, EntityManager entityManager)
    {
        var sunder = entityManager.System<MCXenoSunderSystem>();
        sunder.AddSunder(target, -Amount);
    }
}
