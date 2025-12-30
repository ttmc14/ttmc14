using Content.Shared._MC.CameraShake;
using Content.Shared._MC.Knockback;
using Content.Shared._MC.Stun;
using Content.Shared.Coordinates;
using Content.Shared.Damage;
using Content.Shared.Mobs.Components;
using Robust.Shared.Audio.Systems;

namespace Content.Shared._MC.Xeno.Abilities.Crusher.Stomp;

// TODO: [MC] Use MCXenoAbilitySystem<TComponent, TEvent>
public sealed class MCXenoStompSystem : MCXenoAbilitySystem
{
    [Dependency] private readonly DamageableSystem _damageable = null!;
    [Dependency] private readonly EntityLookupSystem _lookup = null!;
    [Dependency] private readonly SharedAudioSystem _audio = null!;

    [Dependency] private readonly MCCameraShakeSystem _mcCameraShake = null!;
    [Dependency] private readonly MCKnockbackSystem _mcKnockback = null!;
    [Dependency] private readonly MCStunSystem _mcStun = null!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MCXenoStompComponent, MCXenoStompActionEvent>(OnAction);
    }

    private void OnAction(Entity<MCXenoStompComponent> entity, ref MCXenoStompActionEvent args)
    {
        if (args.Handled || !TryUseAction(entity, args.Action))
            return;

        args.Handled = true;

        var coordinates = Transform(entity).Coordinates;
        foreach (var target in _lookup.GetEntitiesInRange<MobStateComponent>(coordinates, entity.Comp.Distance))
        {
            if (entity.Owner == target.Owner)
                continue;

            ProcessHit(entity, target.Owner);
        }

        _audio.PlayPredicted(entity.Comp.EffectSound, entity, entity);
        ServerSpawnAttachedTo(entity.Comp.EffectProtoId, entity.Owner.ToCoordinates());
    }

    private void ProcessHit(Entity<MCXenoStompComponent> entity, EntityUid targetUid)
    {
        if (!ValidateTarget(entity, targetUid))
            return;

        var distance = GetDistance(entity, targetUid);
        var damage = entity.Comp.Damage / Math.Max(1, distance + 1);

        var cameraShake = entity.Comp.CameraShakeEntry;
        var paralyze = entity.Comp.Paralyze;

        if (distance <= entity.Comp.ExtraAffectDistance)
        {
            cameraShake = entity.Comp.ExtraCameraShakeEntry;
            paralyze = entity.Comp.ExtraParalyze;
        }

        _mcKnockback.KnockbackFrom(targetUid, entity, entity.Comp.ThrowEntry);
        _mcStun.Paralyze(targetUid, paralyze);

        _damageable.TryChangeDamage(targetUid, damage, origin: entity, tool: entity);

        _mcCameraShake.ShakeCamera(targetUid, cameraShake);
        RaiseEffect(entity, targetUid);
    }
}
