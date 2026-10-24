using Content.Server._MC.Rules.Common;
using Content.Server.GameTicking.Rules;
using Content.Shared._MC.Living;
using Content.Shared._MC.Rules.EndRound;
using Content.Shared._RMC14.Marines;
using Content.Shared._RMC14.Rules;
using Content.Shared._RMC14.Thunderdome;
using Content.Shared.Mobs;

namespace Content.Server._MC.Rules.EndRound;

public sealed class MCRuleEndRoundMarinesDeadSystem : GameRuleSystem<MCRuleEndRoundMarinesDeadComponent>
{
    [Dependency] private readonly MCRuleXenoMarinesSpawnSystem _mcRuleXenoMarinesSpawn = null!;

    [Dependency] private readonly MCLivingSystem _mcLiving = null!;
    [Dependency] private readonly MCRuleEndRoundSystem _mcRuleEndRound = null!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MarineComponent, MobStateChangedEvent>(OnMobStateChanged);
        SubscribeLocalEvent<MarineComponent, ComponentRemove>(OnCompRemove);
    }

    private void OnMobStateChanged<T>(Entity<T> ent, ref MobStateChangedEvent args) where T : IComponent?
    {
        CheckRoundShouldEnd();
    }

    private void OnCompRemove<T>(Entity<T> ent, ref ComponentRemove args) where T : IComponent?
    {
        CheckRoundShouldEnd();
    }

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

    private void CheckRoundShouldEnd(Entity<MCRuleEndRoundMarinesDeadComponent> entity)
    {
        if (!_mcRuleXenoMarinesSpawn.Spawned)
            return;

        var alive = _mcLiving.Get<MarineComponent>(uid => !HasComp<ThunderdomeMapComponent>(Transform(uid).MapUid));
        if (alive > 0)
            return;

        _mcRuleEndRound.EndRound(entity.Comp.EndRuleId);
    }
}
