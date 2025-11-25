using Content.Server.GameTicking;
using Content.Shared._MC.Rules.Crash;
using Content.Shared._RMC14.Marines;
using Content.Shared._RMC14.Thunderdome;
using Content.Shared.GameTicking.Components;
using Content.Shared.Mobs.Components;
using Robust.Shared.Player;

namespace Content.Server._MC.Rules.Crash;

public sealed partial class MCCrashRuleSystem
{
    protected override void AppendRoundEndText(EntityUid uid,
        MCCrashRuleComponent component,
        GameRuleComponent gameRule,
        ref RoundEndTextAppendEvent args)
    {
        base.AppendRoundEndText(uid, component, gameRule, ref args);

        args.AddLine($"{Loc.GetString($"mc-crash-{component.Result.ToString().ToLower()}")}");
    }

    private void CheckRoundShouldEnd()
    {
        var query = QueryActiveRules();
        while (query.MoveNext(out var uid, out _, out var distress, out var gameRule))
        {
            if (!GameTicker.IsGameRuleAdded(uid, gameRule))
                continue;

            CheckRoundShouldEnd((uid, distress, gameRule));
        }
    }

    private void CheckRoundShouldEnd(Entity<MCCrashRuleComponent, GameRuleComponent> ent)
    {
        var marinesCount = 0;
        var marinesQuery = EntityQueryEnumerator<ActorComponent, MarineComponent, MobStateComponent, TransformComponent>();
        while (marinesQuery.MoveNext(out var marineId, out _, out _, out var mobState, out var xform))
        {
            if (HasComp<ThunderdomeMapComponent>(xform.MapUid))
                continue;

            if (_mobState.IsAlive(marineId, mobState))
                marinesCount++;
        }

        if (marinesCount == 0)
            EndRound(ent, MCCrashRuleResult.MajorXenoVictory);
    }

    private void EndRound(Entity<MCCrashRuleComponent> ent, MCCrashRuleResult result, LocId? customMessage = null)
    {
        var ruleComponent = ent.Comp;
        if (ruleComponent.Result != MCCrashRuleResult.None)
            return;

        ruleComponent.Result = result;
        Dirty(ent);

        switch (ruleComponent.Result)
        {
            case MCCrashRuleResult.None:
            case MCCrashRuleResult.MajorMarineVictory:
            case MCCrashRuleResult.MinorMarineVictory:
            case MCCrashRuleResult.MajorXenoVictory:
            case MCCrashRuleResult.MinorXenoVictory:
            default:
                _roundEnd.EndRound();
                break;
        }
    }

}
