using Content.Shared._MC.Rules;

namespace Content.Server._MC.Rules.Distress;

public sealed partial class MCDistressRuleSystem
{
    private void CheckRoundShouldEnd()
    {

    }

    private void EndRound(Entity<MCDistressSignalRuleComponent> ent, MCDisstressRuleResult result, LocId? customMessage = null)
    {
        if (!RoundCheckEnding)
            return;

        var ruleComponent = ent.Comp;
        if (ruleComponent.Result != MCDisstressRuleResult.None)
            return;

        ruleComponent.Result = result;
        Dirty(ent);

        switch (ruleComponent.Result)
        {
            case MCDisstressRuleResult.None:
            case MCDisstressRuleResult.MajorMarineVictory:
            case MCDisstressRuleResult.MinorMarineVictory:
            case MCDisstressRuleResult.MajorXenoVictory:
            case MCDisstressRuleResult.MinorXenoVictory:
            default:
                _roundEnd.EndRound();
                break;
        }
    }
}
