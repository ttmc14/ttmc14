using Content.Server.GameTicking;
using Content.Server.GameTicking.Rules;
using Content.Server.RoundEnd;
using Content.Shared._MC.Rules.EndRound;
using Content.Shared.GameTicking.Components;

namespace Content.Server._MC.Rules.EndRound;

public sealed class MCRuleEndRoundSystem : GameRuleSystem<MCRuleEndRoundComponent>
{
    [Dependency] private readonly RoundEndSystem _roundEnd = null!;

    protected override void AppendRoundEndText(
        EntityUid uid,
        MCRuleEndRoundComponent component,
        GameRuleComponent gameRule,
        ref RoundEndTextAppendEvent args)
    {
        base.AppendRoundEndText(uid, component, gameRule, ref args);

        var key = $"{component.ResultTextPrefix}{component.Result}";
        args.AddLine(Loc.GetString(key));
    }

    public void EndRound(string result)
    {
        foreach (var gameRule in GameTicker.GetActiveGameRules())
        {
            if (!TryComp<MCRuleEndRoundComponent>(gameRule, out var component))
                continue;

            component.Result = result;
            _roundEnd.EndRound();
        }
    }
}
