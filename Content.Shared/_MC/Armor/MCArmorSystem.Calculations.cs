using Content.Shared._RMC14.Weapons.Ranged;
using Content.Shared.Weapons.Melee;

namespace Content.Shared._MC.Armor;

public sealed partial class MCArmorSystem
{
    public static float ArmorToValue(
        int soft,
        int hard = 0,
        int penetration = 0,
        float sunder = 1f,
        float damage = 0f)
    {
        const float softMax = 100f;
        const float hardMax = 100f;

        var effectiveSoft = Math.Max(0f, soft * sunder - penetration);
        var effectiveHard = Math.Max(0f, hard - penetration);

        var multiplier = 1f - effectiveSoft / softMax;

        if (damage != 0)
            multiplier -= effectiveHard / hardMax / damage;

        return Math.Clamp(multiplier, 0f, 1f);
    }

    private (int, int) GetArmorWithType(EntityUid uid, EntityUid tool)
    {
        if (!TryGetArmor(uid, out var soft, out var hard))
            return (0, 0);

        if (_tag.HasTag(tool, TagBomb))
            return (soft.Bomb, hard.Bomb);

        if (_tag.HasTag(tool, TagMelee) || HasComp<MeleeWeaponComponent>(tool))
            return (soft.Melee, hard.Melee);

        if (_tag.HasTag(tool, TagLaser))
            return (soft.Laser, hard.Laser);

        if (_tag.HasTag(tool, TagAcid))
            return (soft.Acid, hard.Acid);

        if (_tag.HasTag(tool, TagFire))
            return (soft.Fire, hard.Fire);

        if (_tag.HasTag(tool, TagEnergy))
            return (soft.Energy, hard.Energy);

        if (_tag.HasTag(tool, TagBio))
            return (soft.Bio, hard.Bio);

        if (_tag.HasTag(tool, TagBullet) || HasComp<RMCBulletComponent>(tool))
            return (soft.Bullet, hard.Bullet);


        return (0, 0);
    }
}
