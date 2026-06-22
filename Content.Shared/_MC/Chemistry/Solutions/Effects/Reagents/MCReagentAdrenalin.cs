using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.Damage;
using Content.Shared.EntityEffects;
using Content.Shared.Jittering;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using JetBrains.Annotations;
using Robust.Shared.Prototypes;

namespace Content.Shared._MC.Chemistry.Solutions.Effects.Reagents;

[UsedImplicitly]
public sealed partial class MCReagentAdrenalin : MCReagentEffect
{
    private const float Overdose = 6;
    private const float OverdoseCritical = 10;

    private const float SpecialEffectVolume = 2;
    private const float SpecialEffectHealBruteMultiplier = 0.4f;
    private const float SpecialEffectHealBurnMultiplier = 0.2f;

    private static readonly TimeSpan SpecialEffectStaminaCooldown = TimeSpan.FromSeconds(60);
    private static readonly string SpecialEffectStaminaKey = "MCAdrenalinStamina";
    private static readonly TimeSpan SpecialEffectCritCooldown = TimeSpan.FromSeconds(120);
    private static readonly string SpecialEffectCritKey = "MCAdrenalinCrit";

    protected override bool TickProcess => true;

    protected override string ReagentEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
    {
        return
        $"""
        При введении:
        - Восстанавливает 30 [color=#64d1fc]выносливости[/color] (раз в {SpecialEffectStaminaCooldown.TotalMinutes:0} минут).

        Убирает 0.5 секунд сонливости.
        Убирает 2 секунды оглушения.
        Убирает 2 секунды сбития с ног.
        Наносит 1 [color=#759a27]токсинов[/color].

        0 - 10 тиков:
        - Восстанавливает 7.5 [color=#64d1fc]выносливости[/color] за тик.

        11 - 40 тиков:
        - Восстанавливает (текущий тик * 0.75 - 14) [color=#64d1fc]выносливости[/color] за тик.

        После 40 тиков:
        - Наносит 15 [color=#64d1fc]выносливости[/color] урона за тик.

        При объёме выше {Overdose}u:
        - Наносит 1 [color=#759a27]токсинов[/color] урона за тик.

        При объёме выше {OverdoseCritical}u:
        - Наносит 1 [color=#ea0e4d]физического[/color] урона за тик.
        - Наносит 1 [color=#da841d]ожогового[/color] урона за тик.
        - Наносит 1 [color=#759a27]токсинов[/color] урона за тик.

        При введении в [color=#d12d2d]критическом состоянии[/color] и объёме выше или равному {SpecialEffectVolume}u:
        - Мгновенно восстанавливает {SpecialEffectHealBruteMultiplier * 100}% всех [color=#ea0e4d]физических[/color] повреждений.
        - Мгновенно восстанавливает {SpecialEffectHealBurnMultiplier * 100}% всех [color=#da841d]ожоговых[/color] повреждений.
        - Наносит 5 [color=#759a27]токсинов[/color].
        - Вызывает сильную дрожь на 5 секунд.
        - Эффект срабатывает не чаще одного раза в {SpecialEffectCritCooldown.TotalMinutes:0} минут.
        """;
    }

    protected override void OnEffect(EntityEffectReagentArgs args, Solution solution, ReagentPrototype reagent, int tick)
    {
        var uid = args.TargetEntity;
        var volume = solution.GetReagent(new ReagentId(reagent.ID, null)).Quantity;

        StatusEffects.TryAddTime(uid, "StatusEffectDrowsiness", TimeSpan.FromSeconds(-0.5));
        // L.AdjustUnconscious(-2 SECONDS)
        StatusEffects.TryAddTime(uid, "Stun", TimeSpan.FromSeconds(-2));
        StatusEffects.TryAddTime(uid, "KnockedDown", TimeSpan.FromSeconds(-2));

        MCDamageable.AdjustToxLoss(uid, 1);

        switch (tick)
        {
            case < 11:
                MCStamina.ApplyDamage(args.TargetEntity, -7.5f);
                break;

            case < 41:
                MCStamina.ApplyDamage(args.TargetEntity, tick * 0.75f - 14);
                break;

            default:
                MCStamina.ApplyDamage(args.TargetEntity, 15f);
                break;
        }

        if (volume > Overdose)
        {
            MCDamageable.AdjustToxLoss(uid, 1);
        }

        if (volume > OverdoseCritical)
        {
            MCDamageable.AdjustBruteLoss(uid, 1);
            MCDamageable.AdjustBurnLoss(uid, 1);
            MCDamageable.AdjustToxLoss(uid, 1);
        }
    }

    protected override void OnEffectStarted(EntityEffectReagentArgs args, Solution solution, ReagentPrototype reagent)
    {
        var uid = args.TargetEntity;
        var volume = solution.GetReagent(new ReagentId(reagent.ID, null)).Quantity;

        if (!MCSolutionCooldown.IsReady(uid, solution, SpecialEffectStaminaKey))
            return;

        MCStamina.ApplyDamage(uid, -30);

        if (!args.EntityManager.TryGetComponent<MobStateComponent>(uid, out var mobStateComponent))
            return;

        if (!args.EntityManager.TryGetComponent<DamageableComponent>(uid, out var damageableComponent))
            return;

        if (mobStateComponent.CurrentState != MobState.Critical)
            return;

        if (!MCSolutionCooldown.IsReady(uid, solution, SpecialEffectCritKey) || volume.Float() < SpecialEffectVolume)
            return;

        if (damageableComponent.Damage.DamageDict.TryGetValue("MCBrute", out var bruteDamage))
            MCDamageable.AdjustBruteLoss(uid, -bruteDamage.Float() * SpecialEffectHealBruteMultiplier);

        if (damageableComponent.Damage.DamageDict.TryGetValue("MCBurn", out var burnDamage))
            MCDamageable.AdjustBurnLoss(uid, -burnDamage.Float() * SpecialEffectHealBurnMultiplier);

        MCDamageable.AdjustToxLoss(uid, 5f);

        args.EntityManager.System<SharedJitteringSystem>().DoJitter(uid, TimeSpan.FromSeconds(5f), true);

        MCSolutionCooldown.StartCooldown(args.TargetEntity, solution, SpecialEffectCritKey, SpecialEffectCritCooldown);
    }

    protected override void OnEffectFinished(EntityEffectReagentArgs args, Solution solution, ReagentPrototype reagent)
    {
        MCSolutionCooldown.StartCooldown(args.TargetEntity, solution, SpecialEffectStaminaKey, SpecialEffectStaminaCooldown);
    }
}
