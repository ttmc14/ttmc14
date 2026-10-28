using Content.Server._MC.Xeno.Hive;
using Content.Server.GameTicking;
using Content.Server.GameTicking.Rules;
using Content.Shared._MC.Rules.Common;
using Content.Shared._RMC14.WeedKiller;
using Content.Shared._RMC14.Xenonids.Construction.Tunnel;
using Content.Shared.Coordinates;

namespace Content.Server._MC.Rules.Common;

public sealed class MCRuleXenoSetupHiveSystem : GameRuleSystem<MCRuleXenoSetupHiveComponent>
{
    [Dependency] private readonly MCXenoHiveSystem _hive = null!;
    [Dependency] private readonly XenoTunnelSystem _xenoTunnel = null!;

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

        SetupTunnels();
    }

    private void SetupTunnels()
    {
        if (_hive.DefaultHive is not { } hiveUid)
            return;

        var temp = new List<EntityUid>();

        var query = EntityQueryEnumerator<XenoTunnelComponent>();
        while (query.MoveNext(out var ent, out _))
        {
            temp.Add(ent);
        }

        foreach (var uid in temp)
        {
            _xenoTunnel.TryPlaceTunnel(hiveUid, null, uid.ToCoordinates(), out _);
            QueueDel(uid);
        }
    }
}
