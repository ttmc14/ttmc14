using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.EntityEffects;
using Robust.Shared.Prototypes;

namespace Content.Shared._MC.Chemistry.Solutions.Effects.Reagents;

public sealed partial class MCReagentSanguinal : MCReagentEffect
{
    private const float Damage = 1.5f;
    private const float BleedDamage = 3f;

    protected override string ReagentEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
    {
        return
        $"""
        Наносит {Damage} [color=#ea0e4d]физический[/color] и {BleedDamage} [color=#ff0000]кровотечение[/color] за тик.

        Наносит {Damage} [color=#64d1fc]выносливости[/color] за тик, если есть [color=#602cff]хемодайл[/color].
        Наносит {Damage} [color=#759a27]токсины[/color] за тик, если есть [color=#CF3600]нейротоксин[/color].
        Наносит {Damage} [color=#da841d]ожоги[/color] за тик, если есть [color=#94ff00]трансвитокс[/color].
        Наносит {Damage} [color=#1f75d1]удушье[/color] за тик, если есть [color=#f1ddcf]озеломелин[/color]
        """;
    }

    protected override void OnEffect(EntityEffectReagentArgs args, Solution solution, ReagentPrototype reagent, int tick)
    {
        if (HasReagent(solution, "MCHemodile"))
            MCStamina.ApplyDamage(args.TargetEntity, Damage);

        if (HasReagent(solution, "MCNeurotoxin"))
            MCDamageable.AdjustToxLoss(args.TargetEntity, Damage);

        if (HasReagent(solution, "MCTransvitox"))
            MCDamageable.AdjustBurnLoss(args.TargetEntity, Damage);

        if (HasReagent(solution, "MCOzelomelyn"))
            MCDamageable.AdjustOxyLoss(args.TargetEntity, Damage);

        MCDamageable.AdjustBruteLoss(args.TargetEntity, Damage);
        Bloodstream.TryModifyBleedAmount(args.TargetEntity, BleedDamage);
    }
}
