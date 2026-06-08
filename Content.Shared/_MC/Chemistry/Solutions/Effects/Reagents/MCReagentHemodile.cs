using Content.Shared._MC.Stun;
using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.EntityEffects;
using JetBrains.Annotations;
using Robust.Shared.Prototypes;

// ReSharper disable NullCoalescingConditionIsAlwaysNotNullAccordingToAPIContract
// ReSharper disable UseCollectionExpression

namespace Content.Shared._MC.Chemistry.Solutions.Effects.Reagents;

[UsedImplicitly]
public sealed partial class MCReagentHemodile : MCReagentEffect
{
    private const float MultiplierPerMatchingReagent = 0.6f;

    private static readonly List<string> SynergyReagents = new()
    {
        "MCNeurotoxin",
        "MCHemodile",
        "MCTransvitox",
        "MCSanguinal",
        "MCOzelomelyn",
    };

    private MCStunSystem _stun = null!;

    protected override string ReagentEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
    {
        return "Мне лень.";
    }

    protected override void OnInitialize(IEntityManager entityManager)
    {
        _stun ??= entityManager.System<MCStunSystem>();
    }

    protected override void OnEffect(EntityEffectReagentArgs args, Solution solution, ReagentPrototype reagent, int tick)
    {
        _stun.TrySlowdown(args.TargetEntity, "SlowedDown", TimeSpan.FromSeconds(1.25f), GetModifier(solution));
    }

    private static float GetModifier(Solution solution)
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
