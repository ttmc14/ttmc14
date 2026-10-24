using Content.Server.GameTicking.Rules;
using Content.Shared._MC.Rules.EndRound;
using Content.Shared._MC.Xeno.Hive.Events;

namespace Content.Server._MC.Rules.EndRound;

public sealed class MCRuleEndRoundHiveCollapseSystem : GameRuleSystem<MCRuleEndRoundHiveCollapseComponent>
{
    [Dependency] private readonly MCRuleEndRoundSystem _mcRuleEndRound = null!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MCXenoHiveCollapsed>(OnEnd);
    }

    private void OnEnd(ref MCXenoHiveCollapsed ev)
    {
        foreach (var gameRule in GameTicker.GetActiveGameRules())
        {
            if (!TryComp<MCRuleEndRoundHiveCollapseComponent>(gameRule, out var component))
                continue;

            _mcRuleEndRound.EndRound(component.EndRuleId);
        }
    }
}
