using Content.Shared._MC.Damage;
using Content.Shared._MC.Mob.Stamina;
using Content.Shared.Body.Systems;
using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.Damage;
using Content.Shared.EntityEffects;
using Content.Shared.FixedPoint;
using Content.Shared.Popups;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Shared._MC.Chemistry.Effects;

public abstract partial class MCReagentEffect : EntityEffect
{
    #region Cache

    protected IRobustRandom RobustRandom;

    protected SharedPopupSystem Popup;
    protected SharedBloodstreamSystem Bloodstream;

    protected MCSolutionTickerSystem MCSolutionTicker;
    protected MCDamageableSystem MCDamageable;
    protected MCStaminaSystem MCStamina;

    protected bool EffectProcessed;
    protected bool DamagedProcessed;

    #endregion

    private bool _initialized;

    protected abstract void Effect(EntityEffectReagentArgs args, Solution solution, ReagentPrototype reagent);

    protected virtual void GetDamage(EntityUid uid, Solution solution, ReagentPrototype reagent, DamageSpecifier damage)
    {
    }

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

            Effect(reagentArgs, solution, reagent);
        }
        finally
        {
            EffectProcessed = false;
        }
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
        RobustRandom ??= IoCManager.Resolve<IRobustRandom>();
        Popup ??= entityManager.System<SharedPopupSystem>();
        Bloodstream ??= entityManager.System<SharedBloodstreamSystem>();
        MCSolutionTicker ??= entityManager.System<MCSolutionTickerSystem>();
        MCDamageable ??= entityManager.System<MCDamageableSystem>();
        MCStamina ??= entityManager.System<MCStaminaSystem>();
        // ReSharper restore NullCoalescingConditionIsAlwaysNotNullAccordingToAPIContract

        _initialized = true;
    }

    protected static void Purge(Solution solution, ProtoId<ReagentPrototype>[] reagentIds, FixedPoint2 amount)
    {
        foreach (var reagentId in reagentIds)
        {
            Purge(solution, reagentId, amount);
        }
    }

    protected static void Purge(Solution solution, ProtoId<ReagentPrototype> reagentId, FixedPoint2 amount)
    {
        solution.RemoveReagent(reagentId, amount);
    }

    protected static bool HasReagent(Solution solution, string reagentId)
    {
        return solution.ContainsReagent(reagentId, null);
    }
}
