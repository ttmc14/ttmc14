namespace Content.Shared._MC.Weapon.Vali.Events;

[ByRefEvent]
public readonly record struct MCWeaponValiMeleeHitEvent(IReadOnlyList<EntityUid> HitEntities);
