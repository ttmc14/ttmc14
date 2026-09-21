using Content.Shared._MC.Line;
using Content.Shared._MC.Xeno.Abilities.Heal.HealingInfusion.Components;
using Content.Shared._MC.Xeno.Abilities.Heal.HealingInfusion.Events;
using Content.Shared.Coordinates;
using Content.Shared.StatusEffectNew;
using Robust.Shared.Audio.Systems;

namespace Content.Shared._MC.Xeno.Abilities.Heal.HealingInfusion;

public sealed class MCXenoHealingInfusionSystem : MCXenoAbilitySystem
{
    [Dependency] private readonly SharedAudioSystem _audio = null!;
    [Dependency] private readonly SharedStatusEffectsSystem _statusEffects = null!;
    [Dependency] private readonly MCLineSystem _mcLine = null!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MCXenoHealingInfusionComponent, MCXenoHealingInfusionActionEvent>(OnAction);
    }

    private void OnAction(Entity<MCXenoHealingInfusionComponent> entity, ref MCXenoHealingInfusionActionEvent args)
    {
        if (_statusEffects.HasStatusEffect(args.Target, entity.Comp.StatusEffectId))
            return;

        if (!MCXenoHive.FromSameHive(entity.Owner, args.Target) || IsDead(args.Target))
            return;

        if (!RMCActions.TryUseAction(entity.Owner, args.Action, entity.Owner))
            return;

        _statusEffects.TrySetStatusEffectDuration(args.Target,
            entity.Comp.StatusEffectId,
            entity.Comp.StatusEffectDuration);

        _mcLine.SpawnEffect(entity.Comp.RayEffectId, entity.Owner.ToCoordinates(), args.Target.ToCoordinates());
        _audio.PlayPredicted(entity.Comp.Sound, entity, entity);

        SpawnAttachedTo(entity.Comp.EffectProtoId, args.Target.ToCoordinates());

        args.Handled = true;
    }
}
