using Content.Shared._MC.Stun;
using Content.Shared.Damage;
using Content.Shared.FixedPoint;

namespace Content.Shared._MC.Weapon.Vali.Effects;

public sealed partial class MCWeaponReagentSlowdownEffect : MCWeaponReagentEffect
{
    [DataField]
    public TimeSpan SlowdownDuration;

    public override void Apply(EntityUid target, EntityUid user, FixedPoint2 damageAmount, DamageSpecifier damageSpecifier, EntityManager entityManager)
    {
        var stun = entityManager.System<MCStunSystem>();
        stun.Slowdown(target, SlowdownDuration);
    }
}
