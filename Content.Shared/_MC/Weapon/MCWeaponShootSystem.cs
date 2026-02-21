using Content.Shared._CE.ZLevels.Core.EntitySystems;
using Content.Shared._MC.Weapon.ZLevels;
using Content.Shared.Weapons.Ranged.Components;
using Content.Shared.Weapons.Ranged.Events;

namespace Content.Shared._MC.Weapon;

public sealed class MCWeaponShootSystem : EntitySystem
{
    [Dependency] private readonly CESharedZLevelsSystem _zLevels = null!;
    [Dependency] private readonly MCZLevelShootHelperSystem _zHelper = null!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MCWeaponZLevelsShootComponent, AmmoShotEvent>(OnZLevelShoot);
    }

    private void OnZLevelShoot(Entity<MCWeaponZLevelsShootComponent> entity, ref AmmoShotEvent args)
    {
        if (!TryComp<GunComponent>(entity, out var gun) || gun.ShootCoordinates is not { } target)
            return;

        if (!_zLevels.IsVoidAtCoordinates(target, out _))
            return;

        _zHelper.ApplyZPhysics(entity, args.FiredProjectiles, target, gun.ProjectileSpeed);
    }
}
