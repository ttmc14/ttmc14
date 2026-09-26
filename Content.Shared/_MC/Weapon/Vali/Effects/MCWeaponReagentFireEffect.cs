using Content.Shared._RMC14.Atmos;
using Content.Shared.Damage;
using Content.Shared.FixedPoint;

namespace Content.Shared._MC.Weapon.Vali.Effects;

public sealed partial class MCWeaponReagentFireEffect : MCWeaponReagentEffect
{
    [DataField]
    public int Intensity = 30;

    [DataField]
    public int Duration = 5;

    public override void Apply(EntityUid target, EntityUid user, FixedPoint2 damageAmount, DamageSpecifier damageSpecifier, EntityManager entityManager)
    {
        var fire = entityManager.System<SharedRMCFlammableSystem>();
        fire.Ignite(target, Intensity, Duration, null);
    }
}

