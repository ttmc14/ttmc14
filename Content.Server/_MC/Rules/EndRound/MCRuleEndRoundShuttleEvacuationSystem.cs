using Content.Server._MC.Nuke.Bomb;
using Content.Server.GameTicking.Rules;
using Content.Shared._MC.Nuke.Bomb.Events;
using Content.Shared._MC.Rules.EndRound;
using Content.Shared._MC.Shuttle.Events;

namespace Content.Server._MC.Rules.EndRound;

public sealed class MCRuleEndRoundShuttleEvacuationSystem : GameRuleSystem<MCRuleEndRoundShuttleEvacuationComponent>
{
    [Dependency] private readonly MCRuleEndRoundSystem _endRound = null!;
    [Dependency] private readonly MCNukeSystem _nuke = null!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<MCShuttleEvacuationEvent>(OnEvacuated);
        SubscribeLocalEvent<MCNukeDisarmedEvent>(OnDisarmed);
    }

    private void OnEvacuated(MCShuttleEvacuationEvent ev)
    {
        var rules = QueryActiveRules();
        while (rules.MoveNext(out var uid, out _, out var evacuation, out _))
        {
            evacuation.Evacuated = true;
            CheckEvacuation((uid, evacuation));
        }
    }

    private void OnDisarmed(MCNukeDisarmedEvent ev)
    {
        var rules = QueryActiveRules();
        while (rules.MoveNext(out var uid, out _, out var evacuation, out _))
        {
            CheckEvacuation((uid, evacuation));
        }
    }

    private void CheckEvacuation(Entity<MCRuleEndRoundShuttleEvacuationComponent> rule)
    {
        if (!rule.Comp.Evacuated || HasComp<MCRuleEndRoundNukeExplodedComponent>(rule) && _nuke.HasActiveCountdown())
            return;

        _endRound.EndRound(rule.Comp.EndRuleId);
    }
}
