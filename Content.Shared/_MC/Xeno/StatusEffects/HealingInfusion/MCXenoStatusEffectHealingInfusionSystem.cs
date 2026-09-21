using Content.Shared._MC.Xeno.Heal;
using Content.Shared._MC.Xeno.Sunder;
using Content.Shared._RMC14.Aura;
using Content.Shared._RMC14.Xenonids.Weeds;
using Content.Shared.StatusEffectNew;
using Content.Shared.StatusEffectNew.Components;
using Robust.Shared.Timing;

namespace Content.Shared._MC.Xeno.StatusEffects.HealingInfusion;

public sealed class MCXenoStatusEffectHealingInfusionSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = null!;
    [Dependency] private readonly SharedStatusEffectsSystem _statusEffects = null!;
    [Dependency] private readonly MCXenoSunderSystem _mcXenoSunder = null!;
    [Dependency] private readonly MCXenoHealSystem _mcXenoHeal = null!;
    [Dependency] private readonly SharedAuraSystem _rmcAura = null!;

    private EntityQuery<AffectableByWeedsComponent> _affectableQuery;

    public override void Initialize()
    {
        base.Initialize();

        _affectableQuery = GetEntityQuery<AffectableByWeedsComponent>();

        SubscribeLocalEvent<MCXenoStatusEffectHealingInfusionComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<MCXenoStatusEffectHealingInfusionComponent, StatusEffectAppliedEvent>(OnApply);
        SubscribeLocalEvent<MCXenoStatusEffectHealingInfusionComponent, StatusEffectRemovedEvent>(OnRemoved);
        SubscribeLocalEvent<MCXenoStatusEffectHealingInfusionComponent, StatusEffectTimeUpdatedEvent>(OnTimeUpdated);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<MCXenoStatusEffectHealingInfusionComponent, StatusEffectComponent>();
        while (query.MoveNext(out var uid, out var component, out var effectComponent))
        {
            var entity = new Entity<MCXenoStatusEffectHealingInfusionComponent>(uid, component);
            if (component.TickNext > _timing.CurTime)
                continue;

            if (effectComponent.AppliedTo is not { } targetUid)
                continue;

            component.TickNext = component.TickDelay + _timing.CurTime;

            component.HealthDurationRemaining -= component.TickDelay;
            component.SunderDurationRemaining -= component.TickDelay;

            if (component.HealthDurationRemaining <= TimeSpan.Zero || component.SunderDurationRemaining <= TimeSpan.Zero)
            {
                _statusEffects.TryRemoveStatusEffect(targetUid, MetaData(uid).EntityPrototype!);
                continue;
            }

            ProcessHealing(entity, targetUid);
            ProcessSunder(entity, targetUid);
        }
    }

    private void OnStartup(Entity<MCXenoStatusEffectHealingInfusionComponent> entity, ref ComponentStartup args)
    {
        entity.Comp.TickNext = _timing.CurTime + entity.Comp.TickDelay;
        DirtyField(entity.Owner, entity.Comp, nameof(MCXenoStatusEffectHealingInfusionComponent.TickNext));
    }

    private void OnApply(Entity<MCXenoStatusEffectHealingInfusionComponent> entity, ref StatusEffectAppliedEvent args)
    {
        _rmcAura.GiveAura(args.Target, entity.Comp.EffectAuraColor, null, entity.Comp.EffectAuraStrength);
    }

    private void OnRemoved(Entity<MCXenoStatusEffectHealingInfusionComponent> ent, ref StatusEffectRemovedEvent args)
    {
        RemCompDeferred<AuraComponent>(args.Target);
    }

    private static void OnTimeUpdated(Entity<MCXenoStatusEffectHealingInfusionComponent> entity, ref StatusEffectTimeUpdatedEvent args)
    {
        entity.Comp.HealthDurationRemaining += TimeSpan.FromSeconds(5);
        entity.Comp.SunderDurationRemaining += TimeSpan.FromSeconds(5);
    }

    private void ProcessHealing(Entity<MCXenoStatusEffectHealingInfusionComponent> entity, EntityUid targetUid)
    {
        _mcXenoHeal.HealWounds(targetUid, multiplier: 1f, powerScaling: false, baseHeal: entity.Comp.Heal, maxHealthMultiplier: 0.03f);
    }

    private void ProcessSunder(Entity<MCXenoStatusEffectHealingInfusionComponent> entity, EntityUid targetUid)
    {
        if (!_affectableQuery.TryComp(entity, out var affectable) || !affectable.OnXenoWeeds)
            return;

        var recoveryAura = _mcXenoHeal.GetRecoveryAura(entity);
        var sunderHeal = 1.5f * (1f + recoveryAura * 0.05f);

        _mcXenoSunder.AddSunder(targetUid, sunderHeal);
    }
}
