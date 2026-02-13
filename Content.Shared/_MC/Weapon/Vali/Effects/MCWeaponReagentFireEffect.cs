using Content.Shared._MC.Flammable;
using Content.Shared.Damage;
using Content.Shared.FixedPoint;

namespace Content.Shared._MC.Weapon.Vali.Effects;

public sealed partial class MCWeaponReagentFireEffect : MCWeaponReagentEffect
{
    [DataField]
    public float Stacks;

    public override void Apply(EntityUid target, EntityUid user, FixedPoint2 damageAmount, DamageSpecifier damageSpecifier, EntityManager entityManager)
    {
        var fire = entityManager.System<MCSharedFlammableSystem>();
        fire.AdjustFireStacks(target, Stacks, ignite: true);
    }
}
