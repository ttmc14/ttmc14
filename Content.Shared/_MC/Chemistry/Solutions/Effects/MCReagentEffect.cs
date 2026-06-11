using Content.Shared._MC.Chemistry.Solutions.CooldownProvider;
using Content.Shared._MC.Chemistry.Solutions.Ticker;
using Content.Shared._MC.Damage;
using Content.Shared._MC.Mob.Stamina;
using Content.Shared._RMC14.Chemistry.Reagent;
using Content.Shared.Body.Systems;
using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.Damage;
using Content.Shared.EntityEffects;
using Content.Shared.FixedPoint;
using Content.Shared.Popups;
using Content.Shared.StatusEffectNew;
using JetBrains.Annotations;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Shared._MC.Chemistry.Solutions.Effects;

public abstract partial class MCReagentEffect : EntityEffect
{
    #region Cache

    [PublicAPI] protected IRobustRandom RobustRandom;

    [PublicAPI] protected SharedPopupSystem Popup;
    [PublicAPI] protected SharedBloodstreamSystem Bloodstream;
    [PublicAPI] protected SharedStatusEffectsSystem StatusEffects;

    [PublicAPI] protected RMCReagentSystem RMCReagent;

    [PublicAPI] protected MCSolutionCooldownProviderSystem MCSolutionCooldown;
    [PublicAPI] protected MCSolutionTickerSystem MCSolutionTicker;
    [PublicAPI] protected MCDamageableSystem MCDamageable;
    [PublicAPI] protected MCStaminaSystem MCStamina;

    [PublicAPI] protected bool EffectProcessed;
    [PublicAPI] protected bool DamagedProcessed;

    [PublicAPI] protected virtual bool TickProcess => false;

    #endregion

    private bool _initialized;

    protected virtual void OnInitialize(IEntityManager entityManager) { }
    protected abstract void OnEffect(EntityEffectReagentArgs args, Solution solution, ReagentPrototype reagent, int tick);
    protected virtual void OnEffectStarted(EntityEffectReagentArgs args, Solution solution, ReagentPrototype reagent) { }
    protected virtual void OnEffectFinished(EntityEffectReagentArgs args, Solution solution, ReagentPrototype reagent) { }
    protected virtual void GetDamage(EntityUid uid, Solution solution, ReagentPrototype reagent, DamageSpecifier damage) { }

    public override void Effect(EntityEffectBaseArgs args)
    {
        EffectProcessed = true;

        try
        {
            Initialize();

            if (args is not EntityEffectReagentArgs reagentArgs)
                return;

            if (reagentArgs.Source is not { } solution)
                return;

            if (reagentArgs.Reagent is not { } reagent)
                return;

            var tick = 0;
            if (TickProcess)
            {
                tick = MCSolutionTicker.GetTick(reagentArgs.TargetEntity, solution, reagent);
                if (tick == 0)
                    OnEffectStarted(reagentArgs, solution, reagent);
            }

            OnEffect(reagentArgs, solution, reagent, tick);
        }
        finally
        {
            EffectProcessed = false;
        }
    }

    public void EffectFinished(EntityEffectReagentArgs args, Solution solution, ReagentPrototype reagent)
    {
        Initialize();
        OnEffectFinished(args, solution, reagent);
    }

    public void ProcessDamaged(EntityUid uid, Solution solution, ReagentPrototype reagent, DamageSpecifier damage)
    {
        if (EffectProcessed || DamagedProcessed)
            return;

        DamagedProcessed = true;
        Initialize();
        GetDamage(uid, solution, reagent, damage);
        DamagedProcessed = false;
    }

    private void Initialize()
    {
        if (_initialized)
            return;

        var entityManager = IoCManager.Resolve<IEntityManager>();

        // ReSharper disable NullCoalescingConditionIsAlwaysNotNullAccordingToAPIContract

        // Robust
        RobustRandom ??= IoCManager.Resolve<IRobustRandom>();

        // SS14
        Popup ??= entityManager.System<SharedPopupSystem>();
        Bloodstream ??= entityManager.System<SharedBloodstreamSystem>();
        StatusEffects ??= entityManager.System<SharedStatusEffectsSystem>();

        // RMC
        RMCReagent ??= entityManager.System<RMCReagentSystem>();

        // MC
        MCSolutionTicker ??= entityManager.System<MCSolutionTickerSystem>();
        MCSolutionCooldown ??= entityManager.System<MCSolutionCooldownProviderSystem>();
        MCDamageable ??= entityManager.System<MCDamageableSystem>();
        MCStamina ??= entityManager.System<MCStaminaSystem>();

        // ReSharper restore NullCoalescingConditionIsAlwaysNotNullAccordingToAPIContract

        OnInitialize(entityManager);

        _initialized = true;
    }

    [PublicAPI]
    protected static void Purge(Solution solution, ProtoId<ReagentPrototype>[] reagentIds, FixedPoint2 amount)
    {
        foreach (var reagentId in reagentIds)
        {
            Purge(solution, reagentId, amount);
        }
    }

    [PublicAPI]
    protected static void Purge(Solution solution, ProtoId<ReagentPrototype> reagentId, FixedPoint2 amount)
    {
        solution.RemoveReagent(reagentId, amount);
    }

    [PublicAPI]
    protected void PurgeGroups(Solution solution, List<string> groups, FixedPoint2 amount, ReagentPrototype? ignoreReagent = null)
    {
        foreach (var quantity in new List<ReagentQuantity>(solution.Contents))
        {
            if (ignoreReagent is not null && quantity.Reagent.Prototype == ignoreReagent.ID)
                continue;

            var prototype = RMCReagent.Index(quantity.Reagent.Prototype);
            if (!groups.Contains(prototype.Group))
                continue;

            solution.RemoveReagent(quantity.Reagent, amount);
        }
    }

    [PublicAPI]
    protected static bool HasReagent(Solution solution, string reagentId)
    {
        return solution.ContainsReagent(reagentId, null);
    }
}
