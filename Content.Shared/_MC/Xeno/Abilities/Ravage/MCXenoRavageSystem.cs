using System.Numerics;
using Content.Shared._MC.Knockback;
using Content.Shared._MC.Stun;
using Content.Shared._RMC14.CameraShake;
using Content.Shared._RMC14.Emote;
using Content.Shared._RMC14.Entrenching;
using Content.Shared._RMC14.Marines;
using Content.Shared._RMC14.Power;
using Content.Shared._RMC14.Xenonids;
using Content.Shared._RMC14.Xenonids.Hive;
using Content.Shared.Damage;
using Content.Shared.Mobs.Systems;

namespace Content.Shared._MC.Xeno.Abilities.Ravage;

public sealed class MCXenoRavageSystem : MCXenoAbilitySystem<MCXenoRavageComponent, MCXenoRavageActionEvent>
{
    [Dependency] private readonly EntityLookupSystem _entityLookup = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;

    [Dependency] private readonly SharedRMCEmoteSystem _rmcEmote = default!;
    [Dependency] private readonly SharedXenoHiveSystem _rmcHive = default!;
    [Dependency] private readonly RMCCameraShakeSystem _rmcCameraShake = default!;

    [Dependency] private readonly MCKnockbackSystem _mcKnockback = default!;
    [Dependency] private readonly MCStunSystem _mcStun = default!;

    protected override void OnUse(Entity<MCXenoRavageComponent> entity, ref MCXenoRavageActionEvent args)
    {
        var origin = _transform.GetMapCoordinates(entity);

        var position = origin.Position;
        var localRotation = Transform(entity).LocalRotation;

        var rotation = localRotation - Angle.FromDegrees(180);
        var direction = (localRotation - Angle.FromDegrees(90)).ToVec();

        var aabb = new Box2Rotated(new Box2(position.X - 1, position.Y + 1.5f, position.X + 1, position.Y), rotation, position);

        _rmcEmote.TryEmoteWithChat(entity, entity.Comp.Emote);

        foreach (var uid in _entityLookup.GetEntitiesIntersecting(origin.MapId, aabb))
        {
            if (entity.Owner == uid)
                continue;

            if (_rmcHive.FromSameHive(entity.Owner, uid))
                continue;

            if (_mobState.IsDead(uid))
                continue;

            if (!HasComp<BarricadeComponent>(uid) && !HasComp<MarineComponent>(uid) && !HasComp<XenoComponent>(uid) && !HasComp<RMCApcComponent>(uid))
                continue;

            ApplyEffect(uid, entity, direction);
        }
    }

    private void ApplyEffect(EntityUid tragetUid, EntityUid ownerUid, Vector2 direction)
    {
        var damage = GetDamage(ownerUid);
        var armorPiercing = GetArmorPiercing(ownerUid);

        _damageable.TryChangeDamage(tragetUid, damage, origin: ownerUid, tool: ownerUid, armorPiercing: armorPiercing);
        _mcKnockback.Knockback(tragetUid, direction, 1, 5);
        _rmcCameraShake.ShakeCamera(tragetUid, 2, 1);
        _mcStun.Paralyze(tragetUid, TimeSpan.FromSeconds(1));

        RaiseEffect(ownerUid, tragetUid);
    }
}
