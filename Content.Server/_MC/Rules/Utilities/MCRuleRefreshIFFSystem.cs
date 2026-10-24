using Content.Server.GameTicking;
using Content.Server.GameTicking.Rules;
using Content.Shared._MC.Rules.Utilities;
using Content.Shared._RMC14.Weapons.Ranged.IFF;

namespace Content.Server._MC.Rules.Utilities;

public sealed class MCRuleRefreshIFFSystem : GameRuleSystem<MCRuleRefreshIFFComponent>
{
    [Dependency] private readonly GunIFFSystem _gunIFF = null!;
    [Dependency] private readonly MCRelayedSystem _mcRelayed = null!;

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

    private void RulePlayerSpawning(Entity<MCRuleRefreshIFFComponent> entity)
    {
        _mcRelayed.Relay(() =>
        {
            var factions = EntityQueryEnumerator<MarineIFFComponent>();
            while (factions.MoveNext(out var iffId, out _))
            {
                _gunIFF.SetUserFaction(iffId, entity.Comp.Faction);
            }

            entity.Comp.Refreshed = true;
            Dirty(entity);
        });
    }
}
