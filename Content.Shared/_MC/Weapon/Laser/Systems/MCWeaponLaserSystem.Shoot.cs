using System.Linq;
using Content.Shared._MC.Weapon.Laser.Components;
using Content.Shared.Popups;
using Content.Shared.Projectiles;
using Content.Shared.Weapons.Ranged.Components;
using Content.Shared.Weapons.Ranged.Events;
using Robust.Shared.Map;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Components;

namespace Content.Shared._MC.Weapon.Laser.Systems;

public sealed partial class MCWeaponLaserSystem
{
    public void Shoot(Entity<MCWeaponLaserComponent> entity, Entity<GunComponent> gunEntity, EntityUid? user, EntityCoordinates fromCoordinates, EntityCoordinates toCoordinates)
    {
        if (!TryGetAmmo(entity, out var ammoEntity))
        {
            Popup(Loc.GetString("mc-weapon-laser-failed-no-ammo"));
            return;
        }

        if (entity.Comp.Mode is not { } mode)
        {
            Popup(Loc.GetString("mc-weapon-laser-failed-no-mode"));
            return;
        }

        if (!_mcProvidingBattery.Supported)
            return;

        // For the sake of simplicity, instead of the cost per shot,
        // we'll specify the total number of shots and calculate the cost ourselves
        var fireCost = _mcProvidingBattery.GetMaxCharge(ammoEntity) / mode.Shots;
        var charge = _mcProvidingBattery.GetCharge(ammoEntity);

        if (charge < fireCost)
        {
            Popup(Loc.GetString("mc-weapon-laser-failed-low-ammo"));
            return;
        }

        _audio.PlayPvs(gunEntity.Comp.SoundGunshotModified, gunEntity); // _audio.PlayPredicted(gunEntity.Comp.SoundGunshotModified, gunEntity, user);
        _mcProvidingBattery.SetCharge(ammoEntity, charge - fireCost);

        UpdateShots(entity);

        var fromMapCoordinates = _transform.ToMapCoordinates(fromCoordinates);
        var toMapCoordinates = _transform.ToMapCoordinates(toCoordinates);
        var mapDirection = toMapCoordinates.Position - fromMapCoordinates.Position;
        var direction = mapDirection.Normalized();

        var ray = new CollisionRay(fromMapCoordinates.Position, direction, mode.CollisionMask);

        var rayCastResults = _physics.IntersectRay(fromMapCoordinates.MapId, ray, mode.MaxLength, user, false).ToList();
        if (rayCastResults.Count == 0)
            return;

        var result = rayCastResults[0];
        var hit = result.HitEntity;
        var hitCoordinates = Transform(hit).Coordinates;

        _mcLine.SpawnEffect(mode.EffectId, mode.ProjectileId, fromCoordinates, hitCoordinates);

        // FUNNY MOMENT HERE
        var projectileUid = SpawnAtPosition(mode.ProjectileId, fromCoordinates);
        ShootProjectile(projectileUid, gunEntity, user, out var physicsComponent, out var projectileComponent);

        RaiseLocalEvent(entity, new AmmoShotEvent
        {
            FiredProjectiles = new List<EntityUid> { projectileUid },
        });

        _projectile.ProjectileCollide((projectileUid, projectileComponent, physicsComponent), hit);

        return;

        void Popup(string message)
        {
            if (user is null)
                return;

            _popup.PopupPredicted(message, user.Value, user, PopupType.MediumCaution);
        }
    }

    private void ShootProjectile(EntityUid uid, EntityUid? gunUid, EntityUid? user, out PhysicsComponent physicsComponent, out ProjectileComponent projectileComponent)
    {
        physicsComponent = EnsureComp<PhysicsComponent>(uid);
        _physics.SetBodyStatus(uid, physicsComponent, BodyStatus.InAir);

        projectileComponent = EnsureComp<ProjectileComponent>(uid);
        projectileComponent.Weapon = gunUid;

        var shooter = user ?? gunUid;
        if (shooter is not null)
            _projectile.SetShooter(uid, projectileComponent, shooter.Value);
    }
}
