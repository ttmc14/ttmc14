using Content.Shared._MC.Xeno.Plasma.Systems;
using Content.Shared.Damage;
using Content.Shared.FixedPoint;

namespace Content.Shared._MC.Weapon.Vali.Effects;

public sealed partial class MCWeaponReagentPlasmaEffect : MCWeaponReagentEffect
{
    [DataField]
    public float Amount;

    [DataField]
    public float Multiplier;

    public override void Apply(EntityUid target, EntityUid user, FixedPoint2 damageAmount, DamageSpecifier damageSpecifier, EntityManager entityManager)
    {
        var plasma = entityManager.System<MCXenoPlasmaSystem>();
        plasma.TryRemovePlasma(target, Amount + plasma.GetMaxPlasma(target) * Multiplier);
    }
}
