using Content.Shared._MC.Stun.Events;
using Content.Shared._MC.Xeno.Abilities.Ravager.Rage.Components;
using Content.Shared._MC.Xeno.Heal;

namespace Content.Shared._MC.Xeno.Abilities.Ravager.Rage.Systems.Effects;

public sealed class MCXenoRageEffectImmunitySystem : EntitySystem
{
    [Dependency] private readonly MCXenoRageSystem _rage = null!;
    [Dependency] private readonly MCXenoHealSystem _heal = null!;

    public override void Initialize()
    {
        SubscribeLocalEvent<MCXenoRageEffectImmunityComponent, MCStunAttemptEvent>(OnStunAttempt);
        SubscribeLocalEvent<MCXenoRageEffectImmunityComponent, MCStaggerAttemptEvent>(OnStaggerAttempt);
    }

    private void OnStunAttempt(Entity<MCXenoRageEffectImmunityComponent> entity, ref MCStunAttemptEvent args)
    {
        if (!_rage.IsActive(entity) || !_heal.CheckHealthThreshold(entity, entity.Comp.StunImmuneThreshold))
            return;

        args.Canceled = true;
    }

    private void OnStaggerAttempt(Entity<MCXenoRageEffectImmunityComponent> entity, ref MCStaggerAttemptEvent args)
    {
        if (!_rage.IsActive(entity) || !_heal.CheckHealthThreshold(entity, entity.Comp.StaggerImmuneThreshold))
            return;

        args.Canceled = true;
    }
}
