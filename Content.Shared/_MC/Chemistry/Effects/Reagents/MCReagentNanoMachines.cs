using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.EntityEffects;
using Content.Shared.FixedPoint;
using Content.Shared.Popups;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Shared._MC.Chemistry.Effects.Reagents;

public sealed partial class MCReagentNanoMachines : MCReagentEffect
{
    private const float Overdose = 36;
    private const float OverdoseCritical = 85;

    private const float HealBruteAmount = 3;
    private const float HealBurnAmount = 3;
    private const float ToxLossAmount = 0.1f;
    private const float UsePerHeal = 0.5f;

    // ReSharper disable once UseCollectionExpression
    private static readonly ProtoId<ReagentPrototype>[] PurgeReagents = new[]
    {
        new ProtoId<ReagentPrototype>("MCBicaridine"),
        new ProtoId<ReagentPrototype>("MCKelotane"),
        new ProtoId<ReagentPrototype>("MCTramadol"),
        new ProtoId<ReagentPrototype>("MCTricordrazine"),
        new ProtoId<ReagentPrototype>("MCParacetamol"),
        new ProtoId<ReagentPrototype>("MCOxycodone"),
        // new ProtoId<ReagentPrototype>("MCIfosfamide"),
    };

    protected override string ReagentEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
    {
        return Loc.GetString("reagent-effect-mc-nano-machines");
    }

    protected override void Effect(EntityEffectReagentArgs args, Solution solution, ReagentPrototype reagent)
    {
        var tagetUid = args.TargetEntity;
        var tick = MCSolutionTicker.GetTick(tagetUid, solution, reagent);
        var volume = solution.GetReagent(new ReagentId(reagent.ID, null)).Quantity;

        Purge(solution, PurgeReagents, 5f);

        ProcessOverdose(tagetUid, solution, volume, reagent);
        ProcessOverdoseCritical(tagetUid, solution, volume, reagent);

        switch (tick)
        {
            case 1:
            {
                Popup.PopupEntity("Вы чувствуете, что вам следует оставаться поблизости от пункта оказания медицинской помощи, пока инъекция не подействует.", tagetUid, PopupType.SmallCaution);
                return;
            }

            case < 75:
            {
                MCDamageable.AdjustOxyLoss(tagetUid, 1);
                MCStamina.Damage(tagetUid, 1.5f, belowZero: false);

                RegenerateSelf(0.4f);

                if (RobustRandom.Prob(0.05f))
                    Popup.PopupEntity("Вы чувствуете сильный зуд!", tagetUid, PopupType.SmallCaution);

                return;
            }

            case 76:
            {
                Popup.PopupEntity("Боль быстро утихает. Похоже, они к тебе приспособились.", tagetUid, PopupType.SmallCaution);
                return;
            }
        }


        if (volume < 30)
        {
            RegenerateSelf(0.15f);
            Bloodstream.TryModifyBloodLevel(tagetUid, -2f);
        }

        if (volume < 35)
            RegenerateSelf(0.1f);

        // if (volume > 5)
        //    L.reagent_pain_modifier += PAIN_REDUCTION_HEAVY

        ProcessHealBrute(tagetUid, solution, volume, reagent);
        ProcessHealBurn(tagetUid, solution, volume, reagent);

        return;

        void RegenerateSelf(float value)
        {
            solution.AddReagent(reagent.ID, value);
        }
    }

    private void ProcessOverdose(EntityUid uid, Solution solution, FixedPoint2 volume, ReagentPrototype reagent)
    {
        if (volume < Overdose)
            return;

        MCDamageable.AdjustToxLoss(uid, 1f);
        solution.RemoveReagent(reagent.ID, 0.25f);
    }

    private void ProcessOverdoseCritical(EntityUid uid, Solution solution, FixedPoint2 volume, ReagentPrototype reagent)
    {
        if (volume < OverdoseCritical)
            return;

        MCDamageable.AdjustCloneLoss(uid, 1f);
    }

    private void ProcessHealBrute(EntityUid uid, Solution solution, FixedPoint2 volume, ReagentPrototype reagent)
    {
        if (volume <= 5)
            return;

        if (!MCDamageable.HasBruteLoss(uid))
            return;

        MCDamageable.AdjustBruteLoss(uid, -HealBruteAmount);
        MCDamageable.AdjustToxLoss(uid, ToxLossAmount);
        solution.RemoveReagent(reagent.ID, UsePerHeal);

        if (!RobustRandom.Prob(0.4f))
            return;

        Popup.PopupEntity("Ваши порезы и синяки быстро покрываются корками!", uid, PopupType.SmallCaution);
    }

    private void ProcessHealBurn(EntityUid uid, Solution solution, FixedPoint2 volume, ReagentPrototype reagent)
    {
        if (volume <= 5)
            return;

        if (!MCDamageable.HasBurnLoss(uid))
            return;

        MCDamageable.AdjustBurnLoss(uid, -HealBurnAmount);
        MCDamageable.AdjustToxLoss(uid, ToxLossAmount);
        solution.RemoveReagent(reagent.ID, UsePerHeal);

        if (!RobustRandom.Prob(0.4f))
            return;

        Popup.PopupEntity("Ваши ожоги начнут заживать, обнажая здоровую ткань!", uid, PopupType.SmallCaution);
    }
}
