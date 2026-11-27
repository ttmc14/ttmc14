using Content.Shared._MC.Rules.Common;
using Robust.Shared.Player;

namespace Content.Server._MC.Rules.Common;

public sealed class MCRuleReserveGrowthSystem : EntitySystem
{
    [Dependency] private readonly ISharedPlayerManager _players = null!;

    public override void Initialize() =>
        SubscribeLocalEvent<MCRuleReserveGrowthComponent, MCGetReserveGrowthEvent>(OnGetReserveGrowth);

    private void OnGetReserveGrowth(Entity<MCRuleReserveGrowthComponent> rule, ref MCGetReserveGrowthEvent args)
    {
        var groupSize = (long) rule.Comp.MarinesPerXeno + 1;

        var groups = (_players.PlayerCount - rule.Comp.PopulationThreshold) / groupSize;
        if (groups <= 0)
            return;

        rule.Comp.PopulationThreshold += (int) (groups * groupSize);

        var lives = groups * rule.Comp.LivesPerPlayer;

        args.Marines += lives * rule.Comp.MarinesPerXeno;
        args.Xenos += lives;
    }
}
