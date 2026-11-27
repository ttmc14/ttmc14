using Content.Server._MC.Rules.Common;
using Content.Server.GameTicking;
using Content.Shared._MC.Rules.EndRound;

namespace Content.Server._MC.Rules.EndRound;

public sealed class MCRuleEndRoundEliminationSystem : EntitySystem
{
    [Dependency] private readonly GameTicker _ticker = null!;
    [Dependency] private readonly MCRuleEndRoundSystem _roundEnd = null!;

    public override void Initialize()
    {
        SubscribeLocalEvent<MCRuleEndRoundEliminationComponent, MCCheckPopulationVictoryEvent>(OnCheckVictory);
    }

    private void OnCheckVictory(Entity<MCRuleEndRoundEliminationComponent> rule, ref MCCheckPopulationVictoryEvent args)
    {
        if (_ticker.RoundDuration() < rule.Comp.Delay)
            return;

        var result = (args.LivingMarines, args.LivingXenos) switch
        {
            (0, 0) => rule.Comp.Draw,
            (0, _) => rule.Comp.XenoVictory,
            (_, 0) => rule.Comp.MarineVictory,
            _ => null,
        };

        if (result is null)
            return;

        args.RoundOver = true;
        _roundEnd.EndRound(result);
    }
}
