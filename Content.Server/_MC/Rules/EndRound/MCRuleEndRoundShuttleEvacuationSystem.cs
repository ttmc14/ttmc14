using Content.Server.GameTicking.Rules;
using Content.Shared._MC.Rules.EndRound;
using Content.Shared._MC.Shuttle.Events;

namespace Content.Server._MC.Rules.EndRound;

public sealed class MCRuleEndRoundShuttleEvacuationSystem : GameRuleSystem<MCRuleEndRoundShuttleEvacuationComponent>
{
    [Dependency] private readonly MCRuleEndRoundSystem _mcRuleEndRound = null!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MCShuttleEvacuationEvent>(OnEnd);
    }

    private void OnEnd(MCShuttleEvacuationEvent ev)
    {
        foreach (var gameRule in GameTicker.GetActiveGameRules())
        {
            if (!TryComp<MCRuleEndRoundShuttleEvacuationComponent>(gameRule, out var component))
                continue;

            _mcRuleEndRound.EndRound(component.EndRuleId);
        }
    }
}
