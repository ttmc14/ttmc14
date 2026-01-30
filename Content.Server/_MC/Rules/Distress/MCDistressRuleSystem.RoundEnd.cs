using Content.Server.GameTicking;
using Content.Shared._MC.Nuke.Bomb.Events;
using Content.Shared._MC.Rules;
using Content.Shared._MC.Xeno.Hive.Events;
using Content.Shared._RMC14.Dropship;
using Content.Shared._RMC14.Marines;
using Content.Shared._RMC14.Rules;
using Content.Shared._RMC14.Thunderdome;
using Content.Shared.GameTicking.Components;
using Content.Shared.Mobs.Components;
using Robust.Shared.Player;

namespace Content.Server._MC.Rules.Distress;

public sealed partial class MCDistressRuleSystem
{
    protected override void AppendRoundEndText(EntityUid uid,
        MCDistressSignalRuleComponent component,
        GameRuleComponent gameRule,
        ref RoundEndTextAppendEvent args)
    {
        base.AppendRoundEndText(uid, component, gameRule, ref args);
        args.AddLine($"{Loc.GetString($"mc-distress-{component.Result.ToString().ToLower()}")}");
    }

    private void OnNukeExploded(MCNukeExplodedEvent ev)
    {
        foreach (var gameRule in GameTicker.GetActiveGameRules())
        {
            if (!TryComp<MCDistressSignalRuleComponent>(gameRule, out var component))
                continue;

            EndRound((gameRule, component), MCDisstressRuleResult.MajorMarineVictory);
        }
    }

    private void OnHiveCollapsed(ref MCXenoHiveCollapsed ev)
    {
        foreach (var gameRule in GameTicker.GetActiveGameRules())
        {
            if (!TryComp<MCDistressSignalRuleComponent>(gameRule, out var component))
                continue;

            EndRound((gameRule, component), MCDisstressRuleResult.MajorMarineVictory);
        }
    }
    protected override void ActiveTick(EntityUid uid, MCDistressSignalRuleComponent component, GameRuleComponent gameRule, float frameTime)
    {
        base.ActiveTick(uid, component, gameRule, frameTime);

        if (!(Timing.CurTime >= component.NextCheck))
            return;

        component.NextCheck = Timing.CurTime + component.CheckEvery;
        CheckRoundShouldEnd();
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

    private void CheckRoundShouldEnd(Entity<MCDistressSignalRuleComponent, GameRuleComponent> ent)
    {
        var distressComponent = ent.Comp1;
        if (distressComponent.ForceEndAt is not null && Timing.CurTime >= distressComponent.ForceEndAt)
        {
            EndRound(ent, MCDisstressRuleResult.MinorXenoVictory, "rmc-distress-signal-minorxenovictory-timeout");
            return;
        }

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
            EndRound(ent, MCDisstressRuleResult.MajorXenoVictory);
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

    private void OnDropshipHijackStart(ref DropshipHijackStartEvent ev)
    {

    }

    private void OnDropshipHijackLanded(ref DropshipHijackLandedEvent ev)
    {
        var rules = QueryActiveRules();
        var time = Timing.CurTime;

        while (rules.MoveNext(out _, out var rule, out _))
        {
            if (rule.HijackSongPlayed)
                break;

            rule.HijackSongPlayed = true;
            var song = _audio.PlayGlobal(rule.HijackSong, Filter.Broadcast(), true);
            if (song?.Entity is { } songEnt)
                EnsureComp<RMCHijackSongComponent>(songEnt);

            rule.ForceEndAt = time + _forceEndHijackTime;
            break;
        }
    }
}
