using Content.Server.GameTicking.Rules;
using Content.Shared._MC.Respawn;
using Content.Shared._MC.Rules.Common;

namespace Content.Server._MC.Rules.Common;

public sealed class MCRuleRespawnDelaySystem : GameRuleSystem<MCRuleRespawnDelayComponent>
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MCGetRespawnDelayEvent>(OnGetRespawnDelay);
    }

    private void OnGetRespawnDelay(ref MCGetRespawnDelayEvent args)
    {
        var query = QueryActiveRules();
        if (query.MoveNext(out _, out _, out var rule, out _))
            args.Delay = rule.Delay;
    }
}
