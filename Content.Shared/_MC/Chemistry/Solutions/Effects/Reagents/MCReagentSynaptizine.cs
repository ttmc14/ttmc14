using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.EntityEffects;
using Robust.Shared.Prototypes;

// ReSharper disable UseCollectionExpression

namespace Content.Shared._MC.Chemistry.Solutions.Effects.Reagents;

public sealed partial class MCReagentSynaptizine : MCReagentEffect
{
    private const float Overdose = 6;
    private const float OverdoseCritical = 10;

    private const float PurgeReate = 5;
    private static readonly ProtoId<ReagentPrototype>[] PurgeReagents = new[]
    {
        new ProtoId<ReagentPrototype>("MCMindBreaker"),
    };

    private static readonly TimeSpan SpecialEffectCooldown = TimeSpan.FromMinutes(1);

    protected override bool TickProcess => true;

    protected override string ReagentEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
    {
        return
            """
            При введении: 30 [color=#64d1fc]выносливости[/color] (раз в минуту).

            Убирает 0.5 сонливости
            Убирает 2 оглушения
            УБирает 2 сбития с ног
            Наносит 1 [color=#759a27]токсинов[/color].
             0 - 10 тиков: восстанавливает 7.5 [color=#64d1fc]выносливости[/color]
            11 - 40 тиков: восстанавливает (текущий тик * 0.75 - 14) [color=#64d1fc]выносливости[/color]
                >40 тиков: восстанавливает -15 [color=#64d1fc]выносливости[/color]

            [color=#a4885c]Передозировка:[/color]
            Наносит 2 [color=#759a27]токсинов[/color]

            [color=#a4885c]Крит. передозировка:[/color]
            Наносит 2 [color=#f54242]физического[/color]
            Наносит 2 [color=#f59e42]ожогов[/color]
            Наносит 3 [color=#759a27]токсинов[/color]
            """;
    }

    protected override void OnEffect(EntityEffectReagentArgs args, Solution solution, ReagentPrototype reagent, int tick)
    {
        var uid = args.TargetEntity;
        var volume = solution.GetReagent(new ReagentId(reagent.ID, null)).Quantity;

        Purge(solution, PurgeReagents, PurgeReate);

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
            MCDamageable.AdjustToxLoss(uid, 2);
        }

        if (volume > OverdoseCritical)
        {
            MCDamageable.AdjustBruteLoss(uid, 2);
            MCDamageable.AdjustBurnLoss(uid, 2);
            MCDamageable.AdjustToxLoss(uid, 3);
        }
    }

    protected override void OnEffectStarted(EntityEffectReagentArgs args, Solution solution, ReagentPrototype reagent)
    {
        if (!MCSolutionCooldown.IsReady(args.TargetEntity, solution, reagent.ID))
            return;

        MCStamina.ApplyDamage(args.TargetEntity, -30);
    }

    protected override void OnEffectFinished(EntityEffectReagentArgs args, Solution solution, ReagentPrototype reagent)
    {
        if (!MCSolutionCooldown.IsReady(args.TargetEntity, solution, reagent.ID))
            return;

        MCSolutionCooldown.StartCooldown(args.TargetEntity, solution, reagent.ID, SpecialEffectCooldown);
    }
}
