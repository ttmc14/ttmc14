using Content.Shared._MC.Damage;
using Content.Shared._MC.Knockback;
using Content.Shared._MC.Map;
using Content.Shared._MC.Mob.Stamina;
using Content.Shared._MC.Stun;
using Robust.Shared.Audio.Systems;

namespace Content.Shared._MC.Xeno.Abilities.Chimera.WarpBlast;

public sealed class MCXenoWarpBlastSystem : MCXenoAbilitySystem
{
    [Dependency] private readonly SharedAudioSystem _audio = null!;
    [Dependency] private readonly EntityLookupSystem _lookup = null!;

    [Dependency] private readonly MCAnchoredRadiusSystem _mcAnchoredRadius = null!;
    [Dependency] private readonly MCDamageableSystem _mcDamageable = null!;
    [Dependency] private readonly MCKnockbackSystem _mcKnockback = null!;
    [Dependency] private readonly MCStaminaSystem _mcStamina = null!;
    [Dependency] private readonly MCStunSystem _mcStun = null!;

    public override void Initialize()
    {
        SubscribeLocalEvent<MCXenoWarpBlastComponent, MCXenoWarpBlastActionEvent>(OnAction);
    }

    private void OnAction(Entity<MCXenoWarpBlastComponent> entity, ref MCXenoWarpBlastActionEvent args)
    {
        if (args.Handled)
            return;

        if (!TryUseAction(entity, args.Action))
            return;

        var coordinates = Transform(entity).Coordinates;

        var entities = _lookup.GetEntitiesInRange(coordinates, entity.Comp.Range, LookupFlags.Uncontained);
        _mcAnchoredRadius.GetAnchoredInRadius(entities, coordinates, (int) float.Ceiling(entity.Comp.Range));

        foreach (var targetUid in entities)
        {
            if (entity.Owner == targetUid)
                continue;

            if (IsDead(targetUid))
                continue;

            if (MCXenoHive.FromSameHive(entity.Owner, targetUid))
                continue;

            _mcDamageable.DealBombDamage(targetUid, 0, entity.Comp.Damage, origin: entity);
            _mcStamina.ApplyDamage(targetUid, entity.Comp.DamageStamina);
            _mcStun.Paralyze(targetUid, entity.Comp.ParalyzeDuration);
            _mcKnockback.KnockbackFrom(targetUid, entity, entity.Comp.Knockback);

            RaiseEffect(entity, targetUid);
        }

        _audio.PlayPredicted(entity.Comp.EffectSound, entity, entity);

        args.Handled = true;
    }
}
