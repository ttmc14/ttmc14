using Content.Server._MC.Xeno.Hive;
using Content.Server.GameTicking;
using Content.Server.GameTicking.Rules;
using Content.Shared._MC.Rules.Common;
using Content.Shared._RMC14.Marines;
using Content.Shared._RMC14.Thunderdome;
using Content.Shared.GameTicking.Components;
using Content.Shared.Mobs.Systems;
using Robust.Shared.Player;

namespace Content.Server._MC.Rules.Common;

public sealed class MCRulePopulationSystem : GameRuleSystem<MCRulePopulationComponent>
{
    [Dependency] private readonly MCXenoHiveSystem _hive = null!;
    [Dependency] private readonly MobStateSystem _mobState = null!;

    protected override void ActiveTick(EntityUid uid, MCRulePopulationComponent rule, GameRuleComponent gameRule, float frameTime)
    {
        if (GameTicker.RunLevel != GameRunLevel.InRound
            || Timing.CurTime < rule.NextCheck
            || CompOrNull<MCRuleXenoMarinesSpawnComponent>(uid) is { Spawned: false }
            || _hive.DefaultHive is not { } hive)
            return;

        rule.NextCheck = Timing.CurTime + rule.Interval;

        var marines = new List<EntityUid>();

        var query = EntityQueryEnumerator<MarineComponent, ActorComponent>();
        while (query.MoveNext(out var marine, out _, out _))
        {
            if (!_mobState.IsDead(marine) && !HasComp<ThunderdomeMapComponent>(Transform(marine).MapUid))
                marines.Add(marine);
        }

        var livingXenos = _hive.GetLivingXenos(hive, minTier: 0);

        var victory = new MCCheckPopulationVictoryEvent(marines.Count, livingXenos);
        RaiseLocalEvent(uid, ref victory);

        if (victory.RoundOver)
            return;

        var population = new MCPopulationCheckedEvent(hive, marines, livingXenos);
        RaiseLocalEvent(uid, ref population);
    }
}

[ByRefEvent]
public record struct MCCheckPopulationVictoryEvent(int LivingMarines, int LivingXenos)
{
    public bool RoundOver;
}

[ByRefEvent]
public readonly record struct MCPopulationCheckedEvent(EntityUid Hive, IReadOnlyList<EntityUid> Marines, int LivingXenos);
