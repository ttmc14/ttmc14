using Content.Shared._MC.Damage;
using Content.Shared._MC.Stamina;
using Content.Shared.Body.Systems;
using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.Reagent;
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

    #endregion

    protected abstract void Effect(EntityEffectReagentArgs args, Solution solution, ReagentPrototype reagent);

    public override void Effect(EntityEffectBaseArgs args)
    {
        Initialize(args);

        if (args is not EntityEffectReagentArgs reagentArgs)
            return;

        if (reagentArgs.Source is not { } solution)
            return;

        if (reagentArgs.Reagent is not { } reagent)
            return;

        Effect(reagentArgs, solution, reagent);
    }

    private void Initialize(EntityEffectBaseArgs args)
    {
        // ReSharper disable NullCoalescingConditionIsAlwaysNotNullAccordingToAPIContract
        RobustRandom ??= IoCManager.Resolve<IRobustRandom>();
        Popup ??= args.EntityManager.System<SharedPopupSystem>();
        Bloodstream ??= args.EntityManager.System<SharedBloodstreamSystem>();
        MCSolutionTicker ??= args.EntityManager.System<MCSolutionTickerSystem>();
        MCDamageable ??= args.EntityManager.System<MCDamageableSystem>();
        MCStamina ??= args.EntityManager.System<MCStaminaSystem>();
        // ReSharper restore NullCoalescingConditionIsAlwaysNotNullAccordingToAPIContract
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
}
