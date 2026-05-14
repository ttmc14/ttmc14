using Content.Shared.Projectiles;
using Content.Shared.Weapons.Ranged.Events;

namespace Content.Shared._MC.Weapons.Range.DamageMultiplier;

public sealed class MCWeaponRangeDamageMultiplierSystem : EntitySystem
{
    public override void Initialize()
    {
        SubscribeLocalEvent<MCWeaponRangeDamageMultiplierComponent, AmmoShotEvent>(OnAmmoShot);
    }

    private void OnAmmoShot(Entity<MCWeaponRangeDamageMultiplierComponent> weapon, ref AmmoShotEvent args)
    {
        for (var t = args.FiredProjectiles.Count - 1; t >= 0; --t)
        {
            var uid = args.FiredProjectiles[t];
            if (!TryComp<ProjectileComponent>(args.FiredProjectiles[t], out var projectileComponent))
                continue;

            projectileComponent.Damage *= weapon.Comp.Multiplier;
            Dirty(uid, projectileComponent);
        }
    }
}
