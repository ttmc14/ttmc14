using System.Text.Json.Serialization;
using Content.Shared._RMC14.Chemistry.Reagent;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.EntityEffects;
using Content.Shared.FixedPoint;
using Robust.Shared.Prototypes;

namespace Content.Shared._MC.Chemistry.Solutions.Effects;

public sealed partial class MCPurgeGroups : EntityEffect
{
    [DataField, JsonPropertyName("groups")]
    public List<string> Groups = new();

    [DataField, JsonPropertyName("rate")]
    public FixedPoint2 Amount;

    private RMCReagentSystem? _rmcReagent;

    protected override string? ReagentEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
    {
        var reagentSystem = entSys.GetEntitySystem<RMCReagentSystem>();

        var result = $"Выводит {Amount}u группы реагентов: ";
        foreach (var id in Groups)
        {
            result += $"{id}, ";
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

        if (reagentArgs.Reagent is not { } reagent)
            return;

        _rmcReagent ??= IoCManager.Resolve<IEntitySystemManager>().GetEntitySystem<RMCReagentSystem>();

        foreach (var quantity in new List<ReagentQuantity>(source.Contents))
        {
            if (reagent.ID == quantity.Reagent.Prototype)
                continue;

            if (!Groups.Contains(_rmcReagent.Index(quantity.Reagent.Prototype).Group))
                continue;

            source.RemoveReagent(quantity.Reagent, Amount);
        }
    }
}
