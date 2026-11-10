using Content.Shared._MC.Xeno.Constructions.Silo;
using Content.Shared._MC.Xeno.Hive.Components;
using Content.Shared._MC.Xeno.Hive.Systems.Main;
using Content.Shared._RMC14.Xenonids.Hive;
using Content.Shared.Administration.Logs;
using Content.Shared.Coordinates;
using Content.Shared.Database;
using Content.Shared.Mind;
using Robust.Shared.Map;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Random;

namespace Content.Shared._MC.Xeno.Hive.Join;

public sealed class MCXenoHiveJoinSystem : EntitySystem
{
    [Dependency] private readonly ISharedAdminLogManager _adminLog = null!;
    [Dependency] private readonly INetManager _net = null!;
    [Dependency] private readonly IRobustRandom _random = null!;

    [Dependency] private readonly SharedMindSystem _mind = null!;
    [Dependency] private readonly SharedTransformSystem _transform = null!;

    [Dependency] private readonly MCSharedXenoHiveSystem _hive = null!;

    public bool JoinBurrowedLarva(ICommonSession session, Entity<MCXenoHiveComponent> hive)
    {
        if (_net.IsClient)
            return false;

        if (hive.Comp.BurrowedLarva <= 0)
            return false;

        if (!TrySpawn(hive, out var resultUid))
            return false;

        _hive.AddBurrowedLarva((hive, hive), -1);
        _hive.SetHive(resultUid, hive);

        var name = MetaData(resultUid).EntityName;
        var mind = _mind.CreateMind(session.UserId, name);

        _mind.TransferTo(mind, resultUid, ghostCheckOverride: true);
        _adminLog.Add(LogType.RMCBurrowedLarva, $"{session.Name:player} took a burrowed larva from hive {ToPrettyString(hive):hive}.");
        return true;
    }

    private bool TrySpawn(Entity<MCXenoHiveComponent> hive, out EntityUid resultUid)
    {
        resultUid = default;

        if (GetSpawnCoordinates(hive) is not { } coordinates)
            return false;

        resultUid = PredictedSpawnAtPosition(hive.Comp.BurrowedLarvaId, coordinates);
        _transform.AttachToGridOrMap(resultUid);

        return true;
    }

    private EntityCoordinates? GetSpawnCoordinates(Entity<MCXenoHiveComponent> hive)
    {
        var coordinates = new List<EntityCoordinates>();

        var query = EntityQueryEnumerator<MCXenoSiloComponent, HiveMemberComponent>();
        while (query.MoveNext(out var uid, out _, out var hiveMemberComponent))
        {
            if (hiveMemberComponent.Hive is not { } siloHiveUid)
                continue;

            if (siloHiveUid != hive.Owner)
                continue;

            coordinates.Add(uid.ToCoordinates());
        }

        if (coordinates.Count > 0)
            return _random.Pick(coordinates);

        return null;
    }
}
