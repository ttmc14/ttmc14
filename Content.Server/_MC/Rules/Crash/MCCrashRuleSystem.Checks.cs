using Content.Shared._MC.Nuke.Bomb.Events;
using Content.Shared._MC.Rules.Crash;
using Content.Shared._MC.Shuttle.Events;
using Content.Shared._MC.Xeno.Hive.Events;
using Content.Shared._RMC14.Marines;
using Content.Shared._RMC14.Thunderdome;
using Content.Shared.Mobs;

namespace Content.Server._MC.Rules.Crash;

public sealed partial class MCCrashRuleSystem
{
    private void CheckRoundShouldEnd()
    {
        var query = QueryActiveRules();
        while (query.MoveNext(out var uid, out _, out var distress, out var gameRule))
        {
            if (!GameTicker.IsGameRuleAdded(uid, gameRule))
                continue;

            CheckRoundShouldEnd((uid, distress));
        }
    }

    private void CheckRoundShouldEnd(Entity<MCCrashRuleComponent> entity)
    {
        var marinesAlive = _mcLiving.Get<MarineComponent>(uid => !HasComp<ThunderdomeMapComponent>(Transform(uid).MapUid));
        if (marinesAlive > 0)
            return;

        EndRound(entity, MCCrashRuleResult.MajorXenoVictory);
    }

    #region Events

    private void OnNukeExploded(MCNukeExplodedEvent ev)
    {
        EndAllCrashRules(MCCrashRuleResult.MajorMarineVictory);
    }

    private void OnHiveCollapsed(ref MCXenoHiveCollapsed ev)
    {
        EndAllCrashRules(MCCrashRuleResult.MajorMarineVictory);
    }

    private void OnShuttleEvacuationEvent(MCShuttleEvacuationEvent ev)
    {
        EndAllCrashRules(MCCrashRuleResult.MinorXenoVictory);
    }

    #endregion

    #region Marine death

    private void OnMobStateChanged<T>(Entity<T> ent, ref MobStateChangedEvent args) where T : IComponent?
    {
        if (args.NewMobState == MobState.Dead)
            CheckRoundShouldEnd();
    }

    private void OnCompRemove<T>(Entity<T> ent, ref ComponentRemove args) where T : IComponent?
    {
        CheckRoundShouldEnd();
    }

    #endregion
}
