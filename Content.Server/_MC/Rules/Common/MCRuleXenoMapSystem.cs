using Content.Server._MC.Xeno.Spawn;
using Content.Server.GameTicking;
using Content.Server.GameTicking.Rules;
using Content.Shared._MC.Rules.Common;

namespace Content.Server._MC.Rules.Common;

public sealed class MCRuleXenoMapSystem : GameRuleSystem<MCRuleXenoMapComponent>
{
    [Dependency] private readonly MCXenoSpawnSystem _spawn = null!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<LoadingMapsEvent>(OnMapLoading);
        SubscribeLocalEvent<RulePlayerSpawningEvent>(OnRulePlayerSpawning);
    }

    private void OnMapLoading(LoadingMapsEvent ev)
    {
        if (!GameTicker.IsGameRuleAdded<MCRuleXenoMapComponent>())
            return;

        GameTicker.UpdateInfoText();
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

    private void RulePlayerSpawning(Entity<MCRuleXenoMapComponent> entity)
    {
        _spawn.RequestPlanet();

        if (!_spawn.SpawnMap(out var mapUid))
        {
            Log.Error("");
            return;
        }

        entity.Comp.Map = mapUid;
        Dirty(entity);
    }
}
