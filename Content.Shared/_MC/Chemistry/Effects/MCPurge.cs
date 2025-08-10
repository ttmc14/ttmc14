using System.Text.Json.Serialization;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.EntityEffects;
using Content.Shared.FixedPoint;
using Robust.Shared.Prototypes;

namespace Content.Shared._MC.Chemistry.Effects;

public sealed partial class MCPurge : EntityEffect
{
    [DataField, JsonPropertyName("reagents")]
    public List<ProtoId<ReagentPrototype>> Reagents = new();

    [DataField, JsonPropertyName("rate")]
    public FixedPoint2 Amount;

    protected override string? ReagentEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
    {
        return string.Empty;
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
