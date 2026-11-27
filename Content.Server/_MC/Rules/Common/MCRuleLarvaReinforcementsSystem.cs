using Content.Server._MC.Xeno.Hive;
using Content.Server._RMC14.Xenonids.Hive;
using Content.Server.GameTicking;
using Content.Shared._MC.Rules.Common;
using Content.Shared._MC.Xeno.Hive.Systems.Main;
using Content.Shared.Mind;
using Content.Shared.Roles.Jobs;

namespace Content.Server._MC.Rules.Common;

public sealed class MCRuleLarvaReinforcementsSystem : EntitySystem
{
    [Dependency] private readonly GameTicker _ticker = null!;
    [Dependency] private readonly MCXenoHiveSystem _hive = null!;
    [Dependency] private readonly SharedMindSystem _mind = null!;
    [Dependency] private readonly SharedJobSystem _jobs = null!;

    public override void Initialize()
    {
        SubscribeLocalEvent<MCRuleLarvaReinforcementsComponent, MCIssueReserveLarvaeEvent>(OnIssueLarvae);
        SubscribeLocalEvent<BeforeLateJoinLarvaGainEvent>(OnBeforeLateJoinLarvaGain);
    }

    private void OnBeforeLateJoinLarvaGain(ref BeforeLateJoinLarvaGainEvent args)
    {
        if (_ticker.IsGameRuleActive<MCRuleLarvaReinforcementsComponent>())
            args.Cancelled = true;
    }

    private void OnIssueLarvae(Entity<MCRuleLarvaReinforcementsComponent> rule, ref MCIssueReserveLarvaeEvent args)
    {
        if (args.Available <= 0)
            return;

        var points = 0f;
        foreach (var marine in args.Population.Marines)
        {
            var worth = MCSharedXenoHiveSystem.LarvaPointsRegular;
            if (_mind.TryGetMind(marine, out var mind, out _) && _jobs.MindTryGetJobId(mind, out var job) && job is { } jobId)
                worth = rule.Comp.LarvaPointsByJob.GetValueOrDefault(jobId, worth);

            points += worth;
        }

        var target = int.Max(rule.Comp.MinimumPopulation, (int) (points / rule.Comp.PointsPerLarva));

        var needed = target - args.Population.LivingXenos - _hive.GetBurrowedLarva(args.Population.Hive);
        if (needed <= 0)
            return;

        args.Issued = int.Min(needed, args.Available);

        _hive.AddBurrowedLarva(args.Population.Hive, args.Issued);
    }
}
