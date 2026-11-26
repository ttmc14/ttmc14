using System.Linq;
using Content.Shared._MC.Weapon.Laser.Components;
using Content.Shared.Damage.Components;
using Content.Shared.Popups;
using Content.Shared.Projectiles;
using Content.Shared.Weapons.Ranged.Components;
using Content.Shared.Weapons.Ranged.Events;
using Robust.Shared.Map;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Components;
using Robust.Shared.Player;

namespace Content.Shared._MC.Weapon.Laser.Systems;

public sealed partial class MCWeaponLaserSystem
{
    /// <summary>
    /// Fires the laser in its current mode if the installed cell can supply the required charge.
    /// </summary>
    public void Shoot(Entity<MCWeaponLaserComponent> entity, Entity<GunComponent> gunEntity, EntityUid? user,
        EntityCoordinates fromCoordinates, EntityCoordinates toCoordinates, ICommonSession? predictingSession)
    {
        if (_net.IsClient && predictingSession == null)
            return;

        if (!TryGetAmmo(entity, out var ammoEntity))
        {
            // Popup(Loc.GetString("mc-weapon-laser-failed-no-ammo"));
            return;
        }

        if (entity.Comp.Mode is not { } mode)
        {
            // Popup(Loc.GetString("mc-weapon-laser-failed-no-mode"));
            return;
        }

        // For the sake of simplicity, instead of the cost per shot,
        // we'll specify the total number of shots and calculate the cost ourselves
        var fireCost = mode.Consumption ?? _mcBattery.GetMaxCharge(ammoEntity.Owner) / mode.Shots;
        var charge = _mcBattery.GetCharge(ammoEntity.Owner);

        if (charge < fireCost)
        {
            Popup(Loc.GetString("mc-weapon-laser-failed-low-ammo"));
            return;
        }

        if (predictingSession != null)
            _audio.PlayPredicted(gunEntity.Comp.SoundGunshotModified, gunEntity, user);
        else
            _audio.PlayPvs(gunEntity.Comp.SoundGunshotModified, gunEntity);

        if (_net.IsServer)
        {
            _mcBattery.SetCharge(ammoEntity.Owner, charge - fireCost);
            UpdateShots(entity);
        }

        var fromMapCoordinates = _transform.ToMapCoordinates(fromCoordinates);
        var toMapCoordinates = _transform.ToMapCoordinates(toCoordinates);
        var mapDirection = toMapCoordinates.Position - fromMapCoordinates.Position;
        var direction = mapDirection.Normalized();

        var ray = new CollisionRay(fromMapCoordinates.Position, direction, mode.CollisionMask);

        var rayCastResults = _physics.IntersectRay(fromMapCoordinates.MapId, ray, mode.MaxLength, user, false).ToList();
        if (rayCastResults.Count == 0)
            return;

        var result = rayCastResults[0];
        foreach (var collide in rayCastResults)
        {
            if (collide.HitEntity != gunEntity.Comp.Target && CompOrNull<RequireProjectileTargetComponent>(collide.HitEntity)?.Active == true)
                continue;

            result = collide;
            break;
        }

        var hit = result.HitEntity;
        var hitCoordinates = Transform(hit).Coordinates;

        _mcLine.SpawnEffect(mode.EffectId, mode.ProjectileId, fromCoordinates, hitCoordinates, predictingSession);

        if (_net.IsClient)
            return;

        // FUNNY MOMENT HERE
        var projectileUid = SpawnAtPosition(mode.ProjectileId, fromCoordinates);
        ShootProjectile(projectileUid, gunEntity, user, out var physicsComponent, out var projectileComponent);

        RaiseLocalEvent(entity,
            new AmmoShotEvent
            {
                FiredProjectiles = new List<EntityUid> { projectileUid },
            }
        );

        _projectile.ProjectileCollide((projectileUid, projectileComponent, physicsComponent), hit);

        return;

        void Popup(string message)
        {
            if (user is null)
                return;

            _popup.PopupClient(message, user.Value, user, PopupType.MediumCaution);
        }
    }

    private void ShootProjectile(EntityUid uid, EntityUid? gunUid, EntityUid? user, out PhysicsComponent physicsComponent, out ProjectileComponent projectileComponent)
    {
        physicsComponent = EnsureComp<PhysicsComponent>(uid);
        _physics.SetBodyStatus(uid, physicsComponent, BodyStatus.InAir);

        projectileComponent = EnsureComp<ProjectileComponent>(uid);
        _projectile.InitializeShot((uid, projectileComponent), gunUid, user);
    }
}
