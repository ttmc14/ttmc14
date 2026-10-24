using Content.Server._RMC14.Power;
using Content.Server.GameTicking;
using Content.Server.GameTicking.Rules;
using Content.Shared._MC.Rules.Utilities;

namespace Content.Server._MC.Rules.Utilities;

public sealed class MCRuleRecalculatePowerSystem : GameRuleSystem<MCRuleRecalculatePowerComponent>
{
    [Dependency] private readonly MCRelayedSystem _mcRelayed = null!;
    [Dependency] private readonly RMCPowerSystem _rmcPower = null!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<RulePlayerSpawningEvent>(OnRulePlayerSpawning);
    }

    private void OnRulePlayerSpawning(RulePlayerSpawningEvent ev)
    {
        var query = QueryActiveRules();
        while (query.MoveNext(out var uid, out _, out var component, out var gameRule))
        {
            if (!GameTicker.IsGameRuleAdded(uid, gameRule))
                continue;

            RulePlayerSpawning((uid, component));
        }
    }

    private void RulePlayerSpawning(Entity<MCRuleRecalculatePowerComponent> entity)
    {
        _mcRelayed.Relay(() =>
        {
            _rmcPower.RecalculatePower();

            entity.Comp.Recalculated = true;
            Dirty(entity);
        });
    }
}
