using System.Text.Json.Serialization;
using Content.Shared._RMC14.Chemistry.Reagent;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.EntityEffects;
using Content.Shared.FixedPoint;
using Robust.Shared.Prototypes;

namespace Content.Shared._MC.Chemistry.Solutions.Effects;

public sealed partial class MCPurge : EntityEffect
{
    [DataField, JsonPropertyName("reagents")]
    public List<ProtoId<ReagentPrototype>> Reagents = new();

    [DataField, JsonPropertyName("rate")]
    public FixedPoint2 Amount;

    protected override string? ReagentEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
    {
        var reagentSystem = entSys.GetEntitySystem<RMCReagentSystem>();

        var result = $"Выводит {Amount}u реагентов: ";
        foreach (var id in Reagents)
        {
            if (!reagentSystem.TryIndex(id, out var reagent))
                continue;

            result += $"{Loc.GetString(reagent.LocalizedName)}, ";
        }

        result = result.Remove(result.Length - 2, 2);
        result += " из крови";

        return result;
    }

    public override void Effect(EntityEffectBaseArgs args)
    {
        if (args is not EntityEffectReagentArgs reagentArgs)
            return;

        if (reagentArgs.Source is not { } source)
            return;

        foreach (var reagent in Reagents)
        {
            source.RemoveReagent(reagent, Amount);
        }
    }
}
