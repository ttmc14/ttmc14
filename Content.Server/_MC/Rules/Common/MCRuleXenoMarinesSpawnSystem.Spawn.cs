using Content.Server._MC.Xeno.Hive;
using Content.Server.GameTicking;
using Content.Server.Mind;
using Content.Server.Station.Systems;
using Content.Shared._MC.Rules.Common;
using Content.Shared._MC.Xeno.Evolution;
using Content.Shared._RMC14.Spawners;
using Content.Shared._RMC14.Xenonids;
using Content.Shared._RMC14.Xenonids.Evolution;
using Content.Shared.Coordinates;
using Content.Shared.Preferences;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Server._MC.Rules.Common;

public sealed partial class MCRuleXenoMarinesSpawnSystem
{
    [Dependency] private readonly GameTicker _gameTicker = null!;
    [Dependency] private readonly MindSystem _mind = null!;

    [Dependency] private readonly StationJobsSystem _stationJobs = null!;
    [Dependency] private readonly XenoSystem _xeno = null!;
    [Dependency] private readonly XenoEvolutionSystem _evolution = null!;

    [Dependency] private readonly MCXenoHiveSystem _mcHive = null!;
    [Dependency] private readonly MCXenoEvolutionSystem _mcEvolution = null!;

    /// <summary>
    /// Spawns xenomorphs for any remaining slots (overflow).
    /// If slots remain but there are no players, it adds them as burrowed.
    /// </summary>
    private IReadOnlyList<NetUserId> AssignOverflowXenoRoles(
        MCJobRolePair overflow,
        IReadOnlyDictionary<NetUserId,HumanoidCharacterProfile> profiles,
        ref int remainingXenoSlots)
    {
        var assigned = new List<NetUserId>();
        var failed = new HashSet<NetUserId>();

        while (remainingXenoSlots > 0)
        {
            var id = SelectPlayerForJob(overflow.JobId, profiles, failed);
            if (id is null)
                break; // No more players

            if (TrySpawnXeno(id.Value, overflow.EntityId, remainingXenoSlots))
            {
                assigned.Add(id.Value);
                remainingXenoSlots--;
                continue;
            }

            // If the spawn fails, add the player to the list of errors
            // to avoid attempting to spawn them again and entering an infinite loop
            failed.Add(id.Value);
            break;
        }

        if (remainingXenoSlots <= 0 || _mcHive.DefaultHive is not { } hive)
            return assigned;

        // If any slots remain unfilled, add them as burrowed
        _mcHive.AddBurrowedLarva(hive, remainingXenoSlots);

        // Reset, since the slots are now occupied by larvae
        remainingXenoSlots = 0;

        return assigned;
    }

    private bool TrySpawnXeno(
        NetUserId playerId,
        EntProtoId ent,
        int xenos)
    {
        if (_mcHive.DefaultHive is not { } hive)
            return false;

        if (xenos < _mcEvolution.GetLivingRequirement(hive, ent))
            return false;

        if (!_players.TryGetSessionById(playerId, out var player))
            return false;

        _gameTicker.PlayerJoinGame(player);

        var point = _random.Pick(GetEntities<XenoSpawnPointComponent>());
        var xenoEnt = SpawnAtPosition(ent, point.ToCoordinates());

        _xeno.MakeXeno(xenoEnt);
        _mcHive.SetHive(xenoEnt, hive);

        if (TryComp<XenoEvolutionComponent>(xenoEnt, out var evo))
            _evolution.SetPoints((xenoEnt, evo), 100);

        if (!_mind.TryGetMind(playerId, out var mind))
            mind = _mind.CreateMind(playerId);

        _mind.TransferTo(mind.Value, xenoEnt);
        return true;
    }

    private List<EntityUid> GetEntities<TComp>() where TComp : IComponent
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
