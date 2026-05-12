using Content.Shared._MC.CameraShake;
using Content.Shared._MC.Xeno.Abilities.Defiler.ReagentSelector;
using Content.Shared._MC.Xeno.Heal;
using Content.Shared._MC.Xeno.Plasma.Systems;
using Content.Shared._RMC14.Damage.ObstacleSlamming;
using Content.Shared._RMC14.Pulling;
using Content.Shared._RMC14.Stun;
using Content.Shared._RMC14.Xenonids;
using Content.Shared._RMC14.Xenonids.Plasma;
using Content.Shared._RMC14.Xenonids.Sweep;
using Content.Shared.Coordinates;
using Content.Shared.Damage;
using Content.Shared.Effects;
using Content.Shared.Interaction;
using Content.Shared.Mobs.Components;
using Content.Shared.Stunnable;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Timing;

namespace Content.Shared._MC.Xeno.Abilities.Panther.TearingTail;

public sealed class MCXenoTearingTailSystem : MCXenoAbilitySystem
{
    [Dependency] private readonly SharedAudioSystem _audio = null!;
    [Dependency] private readonly DamageableSystem _damageable = null!;
    [Dependency] private readonly EntityLookupSystem _entityLookup = null!;

    [Dependency] private readonly MCXenoHealSystem _mcXenoHeal = null!;
    [Dependency] private readonly MCXenoPlasmaSystem _mcXenoPlasma = null!;
    [Dependency] private readonly MCCameraShakeSystem _mcCameraShake = null!;
    [Dependency] private readonly MCXenoReagentSelectorSystem _mcXenoReagentSelector = null!;

    private readonly HashSet<EntityUid> _hit = new();

    public override void Initialize()
    {
        SubscribeLocalEvent<MCXenoTearingTailComponent, MCXenoTearingTailActionEvent>(OnAction);
    }

    private void OnAction(Entity<MCXenoTearingTailComponent> entity, ref MCXenoTearingTailActionEvent args)
    {
        if (args.Handled)
            return;

        if (!TryUseAction(entity, args.Action))
            return;

        args.Handled = true;

        _audio.PlayPredicted(entity.Comp.EffectSound, entity, entity);
        EnsureComp<XenoSweepingComponent>(entity);

        _hit.Clear();
        _entityLookup.GetEntitiesInRange(entity, entity.Comp.Range, _hit, LookupFlags.Uncontained);

        foreach (var targetUid in _hit)
        {
            if (!ValidateTarget(entity, targetUid))
                continue;

            // Self
            _mcXenoHeal.Heal(entity, entity.Comp.HealthGain);
            _mcXenoPlasma.RegenPlasma(entity, entity.Comp.PlasmaGain);

            // Target
            _damageable.TryChangeDamage(targetUid, entity.Comp.TargetDamage, origin: entity, tool: entity);
            _mcCameraShake.ShakeCamera(targetUid, entity.Comp.TargetCameraShake);
            _mcXenoReagentSelector.TryInjectReagent(entity.Owner, targetUid, entity.Comp.TargetReagentAmount);

            // Effects
            _audio.PlayPredicted(entity.Comp.EffectHitSound, targetUid, entity);

            RaiseEffect(targetUid);
            PredictedSpawnAttachedTo(entity.Comp.EffectHitId, targetUid.ToCoordinates());
        }
    }
}
