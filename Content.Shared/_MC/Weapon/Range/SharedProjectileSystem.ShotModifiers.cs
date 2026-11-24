// ReSharper disable once CheckNamespace
namespace Content.Shared.Projectiles;

public abstract partial class SharedProjectileSystem
{
    /// <summary>Records the weapon, shooter and damage multiplier for a new shot.</summary>
    public void InitializeShot(Entity<ProjectileComponent> projectile, EntityUid? weapon, EntityUid? user)
    {
        projectile.Comp.Weapon = weapon;
        projectile.Comp.Shooter = user ?? weapon;
        projectile.Comp.ShotDamageMultiplier = _guns.GetShooterDamageMultiplier(user);
        Dirty(projectile);
    }
}
