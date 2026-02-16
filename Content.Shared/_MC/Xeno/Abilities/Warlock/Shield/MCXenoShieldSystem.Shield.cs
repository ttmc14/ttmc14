using System.Globalization;
using System.Numerics;
using Content.Shared._MC.Popup;
using Content.Shared._MC.Xeno.Abilities.Warlock.Shield.Components;
using Content.Shared.Coordinates;
using Content.Shared.Popups;
using Content.Shared.Projectiles;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Events;
using Robust.Shared.Spawners;

namespace Content.Shared._MC.Xeno.Abilities.Warlock.Shield;

public sealed partial class MCXenoShieldSystem
{
    private void InitializeShield()
    {
        SubscribeLocalEvent<MCXenoShieldInstanceComponent, StartCollideEvent>(OnShieldStartCollide);
    }

    private void OnShieldStartCollide(Entity<MCXenoShieldInstanceComponent> entity, ref StartCollideEvent args)
    {
        if (TerminatingOrDeleted(entity) || entity.Comp.Terminating)
            return;

        if (!TryComp<PhysicsComponent>(args.OtherEntity, out var physicsComponent))
            return;

        if (TryComp<ProjectileComponent>(args.OtherEntity, out var projectileComponent))
        {
            entity.Comp.Integrity -= projectileComponent.Damage.GetTotal().Float();
            DirtyField(entity, entity.Comp, nameof(MCXenoShieldInstanceComponent.Integrity));

            var percent = entity.Comp.Integrity / entity.Comp.IntegrityMax * 100f;
            _popup.PopupEntServer($"Shield durability: {percent.ToString("F", CultureInfo.InvariantCulture)}%", entity.Comp.OwnerUid, PopupType.MediumCaution);

            if (entity.Comp.Integrity <= 0)
            {
                EndAbility(entity);
                return;
            }
        }

        CatchProjectile(entity, (args.OtherEntity, physicsComponent));
    }

    private void CreateShield(Entity<MCXenoShieldActiveComponent> entity, MCXenoShieldComponent config)
    {
        if (Net.IsClient)
            return;

        var shieldUid = SpawnAttachedTo(config.ShieldEntProtoId, entity.Owner.ToCoordinates().Offset(new Vector2(0, -1)));
        var shieldComponent = EnsureComp<MCXenoShieldInstanceComponent>(shieldUid);

        // Link shield to owner
        shieldComponent.OwnerUid = entity;
        DirtyField(entity, shieldComponent, nameof(MCXenoShieldInstanceComponent.OwnerUid));

        // Link owner to shield
        entity.Comp.ShieldUid = shieldUid;
        DirtyField(entity, entity.Comp, nameof(MCXenoShieldActiveComponent.ShieldUid));
    }

    private void RemoveShield(Entity<MCXenoShieldActiveComponent> entity, float force = 1f)
    {
        if (entity.Comp.ShieldUid is not {} shieldUid)
            return;

        entity.Comp.ShieldUid = null;
        DirtyField(entity, entity.Comp, nameof(MCXenoShieldActiveComponent.ShieldUid));

        if (!TryComp<MCXenoShieldInstanceComponent>(shieldUid, out var shieldInstanceComponent))
            return;

        shieldInstanceComponent.Terminating = true;
        DirtyField(entity, shieldInstanceComponent,  nameof(MCXenoShieldInstanceComponent.Terminating));

        var shieldEntity = (shieldUid, shieldInstanceComponent);
        foreach (var payload in shieldInstanceComponent.Payloads)
        {
            if (TerminatingOrDeleted(payload.ProjectileUid))
                continue;

            if (!TryComp<PhysicsComponent>(payload.ProjectileUid, out var projectilePhysicsComponent))
                continue;

            ResolveProjectile(shieldEntity, (payload.ProjectileUid, projectilePhysicsComponent), payload, force);
        }

        shieldInstanceComponent.Payloads.Clear();
        DirtyField(entity, shieldInstanceComponent, nameof(MCXenoShieldInstanceComponent.Payloads));

        _transform.SetParent(shieldUid, Transform(entity).ParentUid);

        var shieldTimedDespawn = EnsureComp<TimedDespawnComponent>(shieldUid);
        shieldTimedDespawn.Lifetime = 0.25f;

        _physics.SetBodyType(shieldUid, BodyType.Dynamic);
        _mcKnockback.Knockback(shieldUid, (Transform(shieldUid).LocalRotation - double.Pi / 2).ToVec(), 5f, 30f);

        Dirty(entity, shieldTimedDespawn);
    }

    private void CatchProjectile(Entity<MCXenoShieldInstanceComponent> entity, Entity<PhysicsComponent> target)
    {
        var payload = new MCXenoShieldFrozenProjectilePayload(
            target,
            target.Comp.LinearVelocity,
            target.Comp.AngularVelocity,
            CompOrNull<TimedDespawnComponent>(target)?.Lifetime
        );

        entity.Comp.Payloads.Add(payload);
        // DirtyField(entity, entity.Comp, nameof(MCXenoShieldInstanceComponent.Payloads));

        // Stop projectile
        _physics.SetAngularVelocity(target, 0f);
        _physics.SetLinearVelocity(target, Vector2.Zero);

        // Remove despawn shit
        RemComp<TimedDespawnComponent>(target);
    }

    private void ResolveProjectile(Entity<MCXenoShieldInstanceComponent> entity, Entity<PhysicsComponent> target, MCXenoShieldFrozenProjectilePayload payload, float force = 1f)
    {
        // Stop projectile
        _physics.SetAngularVelocity(target, payload.AngularVelocity * force);
        _physics.SetLinearVelocity(target, payload.LinearVelocity * force);

        // Remove despawn shit
        if (payload.Lifetime is { } lifetime)
        {
            var timedDespawnComponent = EnsureComp<TimedDespawnComponent>(target);
            timedDespawnComponent.Lifetime = lifetime;

            Dirty(target, timedDespawnComponent);
        }

        // Change shooter
        if (TryComp<ProjectileComponent>(target, out var projectileComponent) && force < 0f)
        {
            projectileComponent.Shooter = entity.Comp.OwnerUid;
            projectileComponent.Weapon = entity.Comp.OwnerUid;
            Dirty(target, projectileComponent);
        }
    }
}
