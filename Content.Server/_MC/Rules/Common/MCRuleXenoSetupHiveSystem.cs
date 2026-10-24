using Content.Server._MC.Xeno;
using Content.Server._MC.Xeno.Hive;
using Content.Server.GameTicking;
using Content.Server.GameTicking.Rules;
using Content.Shared._MC.Rules.Common;

namespace Content.Server._MC.Rules.Common;

public sealed class MCRuleXenoSetupHiveSystem : GameRuleSystem<MCRuleXenoSetupHiveComponent>
{
    [Dependency] private readonly MCXenoHiveSystem _hive = null!;
    [Dependency] private readonly MCXenoAutoSpawnBalanceSystem _autoSpawnBalance = null!;

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

    private void RulePlayerSpawning(Entity<MCRuleXenoSetupHiveComponent> entity)
    {
        if (_hive.DefaultHive is not { } defaultHive)
        {
            Log.Error("Can't set default hive, it doesnt exists");
            return;
        }

        _hive.SetConfiguration(defaultHive, entity.Comp.Configuration);
        _hive.AddPsypoints(defaultHive, entity.Comp.Psypoints);

        if (entity.Comp.AutoSpawnBalance)
            _autoSpawnBalance.Enable();
    }
}
