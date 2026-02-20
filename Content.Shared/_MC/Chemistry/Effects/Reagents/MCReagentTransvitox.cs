using Content.Shared._MC.Damage;
using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.Damage;
using Content.Shared.EntityEffects;
using Robust.Shared.Prototypes;

namespace Content.Shared._MC.Chemistry.Effects.Reagents;

public sealed partial class MCReagentTransvitox : MCReagentEffect
{
    private const float BurnConversionRate = 1.25f;
    private const float BurnConversionAmount = 1f;
    private const float ToxinDamagePerTick = 2f;
    private const float MultiplierPerMatchingReagent = 2f;
    private const float ExtraToxPerMultiplier = 0.1f;
    private const float TakeDamageMultiplier = 0.1f;

    // ReSharper disable once UseCollectionExpression
    private static readonly List<string> SynergyReagents = new()
    {
        "MCNeurotoxin",
        "MCHemodile",
        // "MCTransvitox",
        "MCSanguinal",
        "MCOzelomelyn",
    };

    protected override string ReagentEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
    {
        return
        $"""
        Наносит {ToxinDamagePerTick} [color=#759a27]токсины[/color] за тик.
        При получении [color=#ea0e4d]физический[/color], наносит дополнительно {TakeDamageMultiplier} [color=#759a27]токсины[/color] от полученого урона.
        Каждый яд токсин увеличивает урон на {ExtraToxPerMultiplier * 100}% (Включая данный)
        Конвертирует [color=#da841d]ожоги[/color] в [color=#759a27]токсины[/color] в соотношении {BurnConversionAmount}:{BurnConversionRate}
        """;
    }

    protected override void OnEffect(EntityEffectReagentArgs args, Solution solution, ReagentPrototype reagent, int tick)
    {
        var multiplier = GetMultiplier(solution);
        MCDamageable.AdjustToxLoss(args.TargetEntity, ToxinDamagePerTick * (1 + ExtraToxPerMultiplier * multiplier));

        if (!MCDamageable.HasBurnLoss(args.TargetEntity))
            return;

        var burnLoss = float.Min(MCDamageable.GetBurnLoss(args.TargetEntity), BurnConversionAmount);

        MCDamageable.AdjustToxLoss(args.TargetEntity, burnLoss * BurnConversionRate);
        MCDamageable.AdjustBurnLoss(args.TargetEntity, -burnLoss);
    }

    protected override void GetDamage(EntityUid uid, Solution solution, ReagentPrototype reagent, DamageSpecifier damage)
    {
        var multiplier = GetMultiplier(solution);
        MCDamageable.AdjustToxLoss(uid, damage.GetBrute() * multiplier * TakeDamageMultiplier);
    }

    private static float GetMultiplier(Solution solution)
    {
        var multiplier = 1f;
        foreach (var (reagentId, _) in solution.Contents)
        {
            if (!SynergyReagents.Contains(reagentId.Prototype))
                continue;

            multiplier *= MultiplierPerMatchingReagent;
        }

        return multiplier;
    }
}
