using Content.Server.GameTicking;
using Content.Shared._MC.Rules.Crash;
using Content.Shared.GameTicking.Components;

namespace Content.Server._MC.Rules.Crash;

public sealed partial class MCCrashRuleSystem
{
    protected override void AppendRoundEndText(
        EntityUid uid,
        MCCrashRuleComponent component,
        GameRuleComponent gameRule,
        ref RoundEndTextAppendEvent args)
    {
        base.AppendRoundEndText(uid, component, gameRule, ref args);

        var key = $"mc-crash-{component.Result.ToString().ToLower()}";
        args.AddLine(Loc.GetString(key));
    }

    private void EndRound(
        Entity<MCCrashRuleComponent> ent,
        MCCrashRuleResult result,
        LocId? customMessage = null)
    {
        if (!RoundCheckEnding)
            return;

        var comp = ent.Comp;

        if (comp.Result != MCCrashRuleResult.None)
            return;

        comp.Result = result;
        Dirty(ent);

        _roundEnd.EndRound();
    }

    private void EndAllCrashRules(MCCrashRuleResult result)
    {
        foreach (var gameRule in GameTicker.GetActiveGameRules())
        {
            if (!TryComp<MCCrashRuleComponent>(gameRule, out var component))
                continue;

            EndRound((gameRule, component), result);
        }
    }
}
