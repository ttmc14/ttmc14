using Content.Shared.Damage;

namespace Content.Shared._MC.Damage.Extensions;

public static class DamageableSystemExtensions
{
    public static DamageSpecifier? TryChangeDamageExt(
        this DamageableSystem system,
        EntityUid? uid,
        DamageSpecifier? damage,
        bool ignoreResistances = false,
        bool interruptsDoAfters = true,
        DamageableComponent? damageable = null,
        EntityUid? origin = null,
        EntityUid? tool = null,
        int armorPiercing = 0)
    {
        if (damage is null)
            return null;

        return system.TryChangeDamage(uid,
            damage,
            ignoreResistances,
            interruptsDoAfters,
            damageable,
            origin,
            tool,
            armorPiercing);
    }

    public static DamageSpecifier? TryHealDamageExt(
        this DamageableSystem system,
        EntityUid? uid,
        DamageSpecifier? damage,
        DamageableComponent? damageable = null,
        EntityUid? origin = null,
        EntityUid? tool = null)
    {
        if (damage is null)
            return null;

        return system.TryChangeDamage(uid,
            -damage,
            true,
            false,
            damageable,
            origin,
            tool,
            0);
    }
}
