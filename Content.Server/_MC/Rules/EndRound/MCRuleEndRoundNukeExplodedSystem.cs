using Content.Server.GameTicking.Rules;
using Content.Shared._MC.Nuke.Bomb.Events;
using Content.Shared._MC.Rules.EndRound;

namespace Content.Server._MC.Rules.EndRound;

public sealed class MCRuleEndRoundNukeExplodedSystem : GameRuleSystem<MCRuleEndRoundNukeExplodedComponent>
{
    [Dependency] private readonly MCRuleEndRoundSystem _mcRuleEndRound = null!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MCNukeExplodedEvent>(OnEnd);
    }

    private void OnEnd(MCNukeExplodedEvent ev)
    {
        foreach (var gameRule in GameTicker.GetActiveGameRules())
        {
            if (!TryComp<MCRuleEndRoundNukeExplodedComponent>(gameRule, out var component))
                continue;

            _mcRuleEndRound.EndRound(component.EndRuleId);
        }
    }
}
