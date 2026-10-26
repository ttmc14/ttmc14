using System.Numerics;
using Content.Shared._MC.Utilities.Math;
using Content.Shared._MC.Xeno.Abilities.Warlock.Shield.Components;
using Content.Shared._RMC14.Projectiles;
using Content.Shared._RMC14.Weapons.Ranged;
using Content.Shared._RMC14.Weapons.Ranged.IFF;
using Content.Shared.Projectiles;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Components;

namespace Content.Shared._MC.Xeno.Abilities.Warlock.Shield;

public partial class MCSharedXenoShieldSystem
{
    private void InitializeProjectile()
    {
        // Pass
    }

    private void ShieldFreezeProjectile(Entity<MCXenoShieldInstanceComponent> entity, EntityUid projectileUid)
    {
        if (!TryFreezeProjectile(entity, projectileUid))
            return;

        entity.Comp.Projectiles.Add(projectileUid);
    }

    private void ShieldResolveProjectiles(Entity<MCXenoShieldInstanceComponent> entity, float linerVelocityMultiplier = -1f)
    {
        foreach (var projectileUid in entity.Comp.Projectiles)
        {
            ReflectProjectile(projectileUid, linerVelocityMultiplier);
            ClaimProjectile(entity, projectileUid);
        }

        entity.Comp.Projectiles.Clear();
    }

    private bool TryFreezeProjectile(EntityUid entityUid, EntityUid projectileUid)
    {
        if (HasComp<MCXenoShieldFrozenProjectileComponent>(projectileUid))
            return true;

        if (!TryComp<PhysicsComponent>(projectileUid, out var physicsComponent) ||
            !TryComp<ProjectileComponent>(projectileUid, out var projectileComponent))
            return false;

        if (MCXenoHive.FromSameHive(entityUid, projectileUid))
            return false;

        if (projectileComponent.Shooter is { } shooterUid && (shooterUid == entityUid || MCXenoHive.FromSameHive(shooterUid, projectileUid)))
            return false;

        var frozenComponent = new MCXenoShieldFrozenProjectileComponent
        {
            LinerVelocity = _physics.GetMapLinearVelocity(projectileUid, physicsComponent),
            AngularVelocity = _physics.GetMapAngularVelocity(projectileUid, physicsComponent),
            BodyStatus = physicsComponent.BodyStatus,
            BodyType = physicsComponent.BodyType,
            CanCollide = physicsComponent.CanCollide,
            Shooter = projectileComponent.Shooter,
            Weapon = projectileComponent.Weapon,
            DeleteOnCollide = projectileComponent.DeleteOnCollide,
            ProjectileSpent = projectileComponent.ProjectileSpent,
        };

        if (TryComp<ProjectileFixedDistanceComponent>(projectileUid, out var fixedDistance))
        {
            // frozenComponent.FixedDistance = true;
            frozenComponent.FixedDistanceRemaining = (fixedDistance.FlyEndTime - _timing.CurTime).EnsureNonNegative();
            frozenComponent.FixedDistanceArcProj = fixedDistance.ArcProj;
            RemCompDeferred<ProjectileFixedDistanceComponent>(projectileUid);
        }

        AddComp(projectileUid, frozenComponent);

        _physics.SetLinearVelocity(projectileUid, Vector2.Zero);
        _physics.SetAngularVelocity(projectileUid, 0f);
        _physics.SetBodyType(projectileUid, BodyType.Static);
        _physics.SetCanCollide(projectileUid, false);

        projectileComponent.DeleteOnCollide = false;
        projectileComponent.ProjectileSpent = false;
        Dirty(projectileUid, projectileComponent);

        RemCompDeferred<DeleteOnCollideComponent>(projectileUid);

        return true;
    }

    private void ReflectProjectile(Entity<MCXenoShieldFrozenProjectileComponent?> projectile, float linerVelocityMultiplier)
    {
        if (!Resolve(projectile, ref projectile.Comp, false))
            return;

        var velocity = projectile.Comp.LinerVelocity * linerVelocityMultiplier;

        _transform.SetWorldRotationNoLerp(projectile.Owner, velocity.ToAngle());

        if (TryComp<PhysicsComponent>(projectile, out var physicsComponent))
        {
            _physics.SetBodyType(projectile, projectile.Comp.BodyType, body: physicsComponent);
            _physics.SetCanCollide(projectile, projectile.Comp.CanCollide, body: physicsComponent);
            _physics.SetBodyStatus(projectile, physicsComponent, BodyStatus.InAir);

            _physics.SetLinearVelocity(projectile, velocity, body: physicsComponent);
            _physics.SetAngularVelocity(projectile, projectile.Comp.AngularVelocity, body: physicsComponent);
        }

        if (TryComp<ProjectileComponent>(projectile, out var projectileComponent))
        {
            projectileComponent.Weapon = projectile.Comp.Weapon;
            projectileComponent.Shooter = projectile.Comp.Shooter;
            projectileComponent.DeleteOnCollide = projectile.Comp.DeleteOnCollide;
            projectileComponent.ProjectileSpent = projectile.Comp.ProjectileSpent;
        }

        if (projectile.Comp.FixedDistance)
        {
            var fixedDistanceComponent = EnsureComp<ProjectileFixedDistanceComponent>(projectile);

            fixedDistanceComponent.FlyEndTime = (projectile.Comp.FixedDistanceRemaining - _timing.CurTime).EnsureNonNegative();
            fixedDistanceComponent.ArcProj = projectile.Comp.FixedDistanceArcProj;
            Dirty(projectile, fixedDistanceComponent);
        }

        RemComp<MCXenoShieldFrozenProjectileComponent>(projectile);
    }

    private void ClaimProjectile(EntityUid entityUid, EntityUid projectileUid)
    {
        if (TryComp<ProjectileComponent>(projectileUid, out var projectileComponent))
        {
            projectileComponent.Weapon = entityUid;
            projectileComponent.Shooter = entityUid;
            Dirty(projectileUid, projectileComponent);
        }

        RemCompDeferred<ProjectileIFFComponent>(projectileUid);
    }
}
