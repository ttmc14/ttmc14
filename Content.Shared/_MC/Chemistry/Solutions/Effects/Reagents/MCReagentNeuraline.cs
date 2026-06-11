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
public sealed partial class MCReagentNeuraline : MCReagentEffect
{
    private const float Overdose = 5;
    private const float OverdoseCritical = 6;

    private const float SpecialEffectVolume = 3;
    private const float SpecialEffectHealMultiplier = 0.2f;
    private static readonly TimeSpan SpecialEffectCooldown = TimeSpan.FromSeconds(300);

    protected override bool TickProcess => true;

    protected override string ReagentEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
    {
        return
        $"""
        Восстанавливает 7.5 [color=#ea0e4d]физического[/color] за тик.
        Восстанавливает 7.5 [color=#da841d]ожогов[/color] за тик.
        Наносит 3.75 [color=#759a27]токсинов[/color] урона за тик.

        Восстанавливает 30 [color=#64d1fc]выносливости[/color].
        Убирает 5 секунд сонливости.
        Убирает 4 секунды оглушения.
        Убирает 4 секунды сбития с ног.

        При объёме выше {Overdose}u:
        - Наносит 2.5 [color=#759a27]токсинов[/color] урона за тик.

        При объёме выше {OverdoseCritical}u:
        - Наносит 10 [color=#b38bff]клонового[/color] урона за тик.
        - Наносит урон мозгу.

        При введении в [color=#d12d2d]критическом состоянии[/color] и объёме выше или равному {SpecialEffectVolume}u:
        - Мгновенно восстанавливает {SpecialEffectHealMultiplier * 100}% всех [color=#ea0e4d]физических[/color] и [color=#da841d]ожоговых[/color] повреждений.
        - Вызывает сильную дрожь на 5 секунд.
        - Эффект срабатывает не чаще одного раза в {SpecialEffectCooldown.TotalMinutes:0} минут.
        """;
    }

    protected override void OnEffect(EntityEffectReagentArgs args, Solution solution, ReagentPrototype reagent, int tick)
    {
        var uid = args.TargetEntity;
        var volume = solution.GetReagent(new ReagentId(reagent.ID, null)).Quantity;

        MCDamageable.AdjustBruteLoss(uid, -7.5f);
        MCDamageable.AdjustBurnLoss(uid, -7.5f);
        MCDamageable.AdjustToxLoss(uid, 3.75f);

        StatusEffects.TryAddTime(uid, "StatusEffectDrowsiness", TimeSpan.FromSeconds(-5));
        // L.AdjustUnconscious(-2 SECONDS)
        StatusEffects.TryAddTime(uid, "Stun", TimeSpan.FromSeconds(-4));
        StatusEffects.TryAddTime(uid, "KnockedDown", TimeSpan.FromSeconds(-4));
        MCStamina.ApplyDamage(uid, -30f);

        if (volume > Overdose)
        {
            MCDamageable.AdjustToxLoss(uid, 2.5f);
        }

        if (volume > OverdoseCritical)
        {
            MCDamageable.AdjustCloneLoss(uid, 10f);
            // TODO: 10 brain loss
        }
    }

    protected override void OnEffectStarted(EntityEffectReagentArgs args, Solution solution, ReagentPrototype reagent)
    {
        var uid = args.TargetEntity;
        var volume = solution.GetReagent(new ReagentId(reagent.ID, null)).Quantity;

        if (!args.EntityManager.TryGetComponent<MobStateComponent>(uid, out var mobStateComponent))
            return;

        if (!args.EntityManager.TryGetComponent<DamageableComponent>(uid, out var damageableComponent))
            return;

        if (mobStateComponent.CurrentState != MobState.Critical)
            return;

        if (!MCSolutionCooldown.IsReady(uid, solution, reagent.ID) || volume.Float() < SpecialEffectVolume)
            return;

        if (damageableComponent.Damage.DamageDict.TryGetValue("MCBrute", out var bruteDamage))
            MCDamageable.AdjustBruteLoss(uid, -bruteDamage.Float() * SpecialEffectHealMultiplier);

        if (damageableComponent.Damage.DamageDict.TryGetValue("MCBurn", out var burnDamage))
            MCDamageable.AdjustBurnLoss(uid, -burnDamage.Float() * SpecialEffectHealMultiplier);


        args.EntityManager.System<SharedJitteringSystem>().DoJitter(uid, TimeSpan.FromSeconds(5f), true);

        MCSolutionCooldown.StartCooldown(args.TargetEntity, solution, reagent.ID, SpecialEffectCooldown);
    }
}
