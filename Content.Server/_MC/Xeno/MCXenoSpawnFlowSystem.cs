using Content.Server._MC.Xeno.Hive;
using Content.Server._RMC14.Xenonids.Hive;
using Content.Server.GameTicking;
using Content.Server.Mind;
using Content.Shared._RMC14.Spawners;
using Content.Shared._RMC14.Xenonids;
using Content.Shared._RMC14.Xenonids.Evolution;
using Content.Shared.Coordinates;
using Content.Shared.Roles;
using Robust.Server.Player;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Server._MC.Xeno;

public sealed class MCXenoSpawnFlowSystem : EntitySystem
{
    [Dependency] private readonly IPlayerManager _players = null!;
    [Dependency] private readonly IRobustRandom _random = null!;

    [Dependency] private readonly GameTicker _gameTicker = null!;
    [Dependency] private readonly MindSystem _mind = null!;
    [Dependency] private readonly XenoSystem _xeno = null!;
    [Dependency] private readonly XenoEvolutionSystem _evolution = null!;

    [Dependency] private readonly MCXenoHiveSystem _mcHive = null!;
    [Dependency] private readonly MCXenoRoleSelectionSystem _roleSelect = null!;

    public void Spawn(
        RulePlayerSpawningEvent ev,
        ProtoId<JobPrototype> shrikeJob,
        EntProtoId shrikeEnt,
        ProtoId<JobPrototype> xenoSelectableJob,
        EntProtoId larvaEnt,
        int xenos)
    {
        var spawnPoints = GetEntities<XenoSpawnPointComponent>();
        var used = new HashSet<NetUserId>();

        var shrike = _roleSelect.SelectPlayer(ev, shrikeJob, used);
        if (shrike is not null && TrySpawnXeno(ev, shrike.Value, shrikeEnt, spawnPoints))
        {
            used.Add(shrike.Value);
            xenos--;
        }

        SpawnLarvaWithFallback(
            ev,
            xenoSelectableJob,
            larvaEnt,
            spawnPoints,
            used,
            xenos);
    }


    public bool TrySpawnXeno(
        RulePlayerSpawningEvent ev,
        NetUserId playerId,
        EntProtoId ent,
        IReadOnlyList<EntityUid> spawnPoints)
    {
        if (!_players.TryGetSessionById(playerId, out var player))
            return false;

        ev.PlayerPool.Remove(player);
        _gameTicker.PlayerJoinGame(player);

        var point = _random.Pick(spawnPoints);
        var xenoEnt = SpawnAtPosition(ent, point.ToCoordinates());

        _xeno.MakeXeno(xenoEnt);
        _mcHive.SetHive(xenoEnt, _mcHive.DefaultHive);

        if (TryComp<XenoEvolutionComponent>(xenoEnt, out var evo))
            _evolution.SetPoints((xenoEnt, evo), 100);

        if (!_mind.TryGetMind(playerId, out var mind))
            mind = _mind.CreateMind(playerId);

        _mind.TransferTo(mind.Value, xenoEnt);
        return true;
    }

    public void SpawnLarvaWithFallback(
        RulePlayerSpawningEvent ev,
        ProtoId<JobPrototype> larvaJob,
        EntProtoId larvaEnt,
        IReadOnlyList<EntityUid> spawnPoints,
        HashSet<NetUserId> used,
        int targetCount)
    {
        var spawned = SpawnLarvaBatch(ev, larvaJob, larvaEnt, spawnPoints, used, targetCount);

        var unfilled = targetCount - spawned;
        if (unfilled > 0 && _mcHive.DefaultHive is { } hive)
            _mcHive.AddBurrowedLarva(hive, unfilled);
    }

    public int SpawnLarvaBatch(
        RulePlayerSpawningEvent ev,
        ProtoId<JobPrototype> larvaJob,
        EntProtoId larvaEnt,
        IReadOnlyList<EntityUid> spawnPoints,
        HashSet<NetUserId> used,
        int targetCount)
    {
        var spawned = 0;
        while (spawned < targetCount)
        {
            var larva = _roleSelect.SelectPlayer(ev, larvaJob, used);
            if (larva is null)
                break;

            if (!TrySpawnXeno(ev, larva.Value, larvaEnt, spawnPoints))
                continue;

            used.Add(larva.Value);
            spawned++;
        }

        return spawned;
    }

    public List<EntityUid> GetEntities<TComp>() where TComp : IComponent
    {
        var list = new List<EntityUid>();
        var query = AllEntityQuery<TComp>();

        while (query.MoveNext(out var uid, out _))
        {
            list.Add(uid);
        }

        return list;
    }
}
