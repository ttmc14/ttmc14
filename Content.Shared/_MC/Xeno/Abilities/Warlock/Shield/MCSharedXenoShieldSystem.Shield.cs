using System.Globalization;
using System.Numerics;
using Content.Shared._MC.Popup;
using Content.Shared._MC.Xeno.Abilities.Warlock.Shield.Components;
using Content.Shared.Coordinates;
using Content.Shared.Popups;
using Content.Shared.Projectiles;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Events;
using Robust.Shared.Spawners;

namespace Content.Shared._MC.Xeno.Abilities.Warlock.Shield;

public partial class MCSharedXenoShieldSystem
{
    private void InitializeShield()
    {
        SubscribeLocalEvent<MCXenoShieldInstanceComponent, StartCollideEvent>(OnShieldStartCollide);
    }

    private void OnShieldStartCollide(Entity<MCXenoShieldInstanceComponent> entity, ref StartCollideEvent args)
    {
        if (Net.IsClient)
            return;

        if (TerminatingOrDeleted(entity) || entity.Comp.Terminating)
            return;

        if (TryComp<ProjectileComponent>(args.OtherEntity, out var projectileComponent))
        {
            entity.Comp.Integrity -= projectileComponent.Damage.GetTotal().Float();
            DirtyField(entity, entity.Comp, nameof(MCXenoShieldInstanceComponent.Integrity));

            var percent = entity.Comp.Integrity / entity.Comp.IntegrityMax * 100f;
            _popup.PopupEntServer($"Shield durability: {percent.ToString("0.#", CultureInfo.InvariantCulture)}%", entity.Comp.OwnerUid, PopupType.MediumCaution);

            if (entity.Comp.Integrity <= 0)
            {
                EndAbility(entity, forced: true);
                return;
            }
        }

        ShieldFreezeProjectile(entity, args.OtherEntity);
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

        ShieldResolveProjectiles((entity, shieldInstanceComponent), force);

        _transform.SetParent(shieldUid, Transform(entity).ParentUid);

        var shieldTimedDespawn = EnsureComp<TimedDespawnComponent>(shieldUid);
        shieldTimedDespawn.Lifetime = 0.25f;

        _physics.SetBodyType(shieldUid, BodyType.Dynamic);
        _mcKnockback.Knockback(shieldUid, (Transform(shieldUid).LocalRotation - double.Pi / 2).ToVec() * -force, 5f, 30f);

        Dirty(entity, shieldTimedDespawn);
    }
}
