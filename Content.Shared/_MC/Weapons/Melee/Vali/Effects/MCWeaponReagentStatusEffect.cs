using Content.Shared._MC.Weapon.Vali.Effects;
using Content.Shared._MC.Weapons.Projectiles.StatusEffects;
using Content.Shared.Damage;
using Content.Shared.FixedPoint;
using Content.Shared.StatusEffectNew;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._MC.Weapons.Melee.Vali.Effects;

public sealed partial class MCWeaponReagentStatusEffect : MCWeaponReagentEffect
{
    [DataField]
    public List<MCProjectileStatusEffectEntry> StatusEffects = new();

    public override void Apply(EntityUid target, EntityUid user, FixedPoint2 damageAmount, DamageSpecifier damageSpecifier, EntityManager entityManager)
    {
        var system = entityManager.System<SharedStatusEffectsSystem>();
        foreach (var entry in StatusEffects)
        {
            system.TrySetStatusEffectDuration(target, entry.EffectId, entry.Duration);
        }
    }
}

[DataDefinition, Serializable, NetSerializable]
public partial struct MCValiStatusEffectEntry
{
    [DataField]
    public EntProtoId EffectId;

    [DataField]
    public TimeSpan? Duration;
}
