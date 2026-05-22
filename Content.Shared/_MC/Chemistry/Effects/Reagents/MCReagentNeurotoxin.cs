using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.EntityEffects;
using Content.Shared.Jittering;
using JetBrains.Annotations;
using Robust.Shared.Prototypes;

namespace Content.Shared._MC.Chemistry.Effects.Reagents;

[UsedImplicitly]
public sealed partial class MCReagentNeurotoxin : MCReagentEffect
{
    private const float PurgeAmount = 1.5f;

    private static readonly List<string> PurgedGroups = new()
    {
        "Medicine",
    };

    protected override bool TickProcess => true;

    protected override string ReagentEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
    {
        return
        """
        1 - 19 тиков:
        - Наносит 2 [color=#64d1fc]выносливости[/color] за тик.
        - Вызывает боль +15.
        - Выводит 1.5u всех [color=#55ff81]медикаментов[/color] за тик.

        22 - 44 тиков:
        - Наносит 6 [color=#64d1fc]выносливости[/color] за тик.
        - Вызывает боль +35.
        - Вызывает дрожь, одурманенность и размытое зрение.
        - Выводит 3.75u всех [color=#55ff81]медикаментов[/color] за тик.

        После 46 тиков:
        - Наносит 15 [color=#64d1fc]выносливости[/color] за тик.
        - Вызывает боль +50.
        - Усиливает дрожь, одурманенность и размытое зрение.
        - Выводит 6.75u всех [color=#55ff81]медикаментов[/color] за тик.

        Если урон по [color=#64d1fc]выносливости[/color] превышает доступную выносливость:
        - Избыточный урон делится между [color=#759a27]токсинами[/color] и [color=#1f75d1]удушьем[/color].
        - Вызывает остановку дыхания.
        """;
    }

    protected override void OnEffect(EntityEffectReagentArgs args, Solution solution, ReagentPrototype reagent, int tick)
    {
        var target = args.TargetEntity;

        var power = 0f;
        var purgePower = 0f;

        ProcessCycle(args.EntityManager, target, tick, ref power, ref purgePower);
        PurgeGroups(solution, PurgedGroups, purgePower);

        var staminaLossLimit = 100;
        var appliedDamage = float.Clamp(power, 0, staminaLossLimit - MCStamina.GetLoss(target));
        var damageOverflow = power - appliedDamage;

        MCStamina.ApplyDamage(target, appliedDamage, forceTimer: true); // No stamina regeneration

        if (damageOverflow > 0)
        {
            MCDamageable.AdjustToxLoss(target, damageOverflow * 0.5f);
            MCDamageable.AdjustOxyLoss(target, damageOverflow * 0.5f);
        }

        //  L.set_timed_status_effect(2 SECONDS, /datum/status_effect/speech/stutter, only_if_higher = TRUE)

        if (tick < 21)
            return;

        // L.adjust_drugginess(1.1) //Move this to stage 2 and 3 so it's not so obnoxious
        // if(L.eye_blurry < 30) //So we don't have the visual acuity of Mister Magoo forever
        //    L.adjust_blurriness(1.3)
    }

    private void ProcessCycle(IEntityManager manager, EntityUid uid, int tick, ref float power, ref float purgePower)
    {
        const float effectStrength = 1f; // TODO

        var jittering = manager.System<SharedJitteringSystem>();

        if (tick is > 0 and < 20)
        {
            power = 2 * effectStrength;
            purgePower = PurgeAmount;

            // L.reagent_pain_modifier -= PAIN_REDUCTION_LIGHT
            return;
        }

        if (tick is > 21 and < 45)
        {
            power = 6 * effectStrength;
            purgePower = PurgeAmount * 2.5f;

            // L.reagent_pain_modifier -= PAIN_REDUCTION_HEAVY
            jittering.DoJitter(uid, TimeSpan.FromSeconds(1), true, frequency: 6);
            return;
        }

        if (tick > 46)
        {
            power = 15 * effectStrength;
            purgePower = PurgeAmount * 4.5f;

            // L.reagent_pain_modifier -= PAIN_REDUCTION_VERY_HEAVY
            jittering.DoJitter(uid, TimeSpan.FromSeconds(1), true, frequency: 6);
            return;
        }
    }
}
