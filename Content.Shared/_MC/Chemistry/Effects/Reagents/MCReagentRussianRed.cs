using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.Damage;
using Content.Shared.EntityEffects;
using Content.Shared.Jittering;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using JetBrains.Annotations;
using Robust.Shared.Prototypes;

namespace Content.Shared._MC.Chemistry.Effects.Reagents;

[UsedImplicitly]
public sealed partial class MCReagentRussianRed : MCReagentEffect
{
    private const float Overdose = 15;
    private const float OverdoseCritical = 20;

    private const float SpecialEffectVolume = 9;
    private const float SpecialEffectHealMultiplier = 0.2f;
    private static readonly TimeSpan SpecialEffectCooldown = TimeSpan.FromSeconds(300);

    protected override bool TickProcess => true;

    protected override string ReagentEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
    {
        return
        $"""
        Восстанавливает 7 [color=#ea0e4d]физический[/color] за тик.
        Восстанавливает 7 [color=#da841d]ожоги[/color] за тик.
        Восстанавливает 2.5 [color=#759a27]токсинов[/color] за тик.
        Наносит 0.7 [color=#b38bff]клон[/color] урона за тик.

        При объёме выше {Overdose}u:
        - Наносит 1 [color=#ea0e4d]физического[/color] урона за тик.

        При объёме выше {OverdoseCritical}u:
        - Наносит 1 [color=#ea0e4d]физического[/color] урона за тик.
        - Наносит 2 [color=#da841d]ожогового[/color] урона за тик.
        - Наносит 1 [color=#759a27]токсинов[/color] урона за тик.

        При введении в [color=#d12d2d]критическом состоянии[/color] и объёме выше или равному {SpecialEffectVolume}u:
        - Мгновенно восстанавливает {SpecialEffectHealMultiplier * 100}% всех [color=#ea0e4d]физический[/color] и [color=#da841d]ожоги[/color] повреждений.
        - Вызывает сильную дрожь на 5 секунд.
        - Эффект срабатывает не чаще одного раза в {SpecialEffectCooldown.TotalMinutes:0} минут
        """;
    }

    protected override void OnEffect(EntityEffectReagentArgs args, Solution solution, ReagentPrototype reagent, int tick)
    {
        var uid = args.TargetEntity;
        var volume = solution.GetReagent(new ReagentId(reagent.ID, null)).Quantity;

        MCDamageable.AdjustBruteLoss(uid, -7);
        MCDamageable.AdjustBurnLoss(uid, -7);
        MCDamageable.AdjustToxLoss(uid, -2.5f);
        MCDamageable.AdjustCloneLoss(uid, 0.7f);

        if (volume > Overdose)
        {
            MCDamageable.AdjustBruteLoss(uid, 1f);
        }

        if (volume > OverdoseCritical)
        {
            MCDamageable.AdjustBruteLoss(uid, 1);
            MCDamageable.AdjustBurnLoss(uid, 2);
            MCDamageable.AdjustToxLoss(uid, 1);
            // TODO: brain loss
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
