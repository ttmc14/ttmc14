using Content.Server._MC.Squad;
using Content.Server.Shuttles.Systems;
using Content.Server.Spawners.Components;
using Content.Server.Spawners.EntitySystems;
using Content.Server.Station.Systems;
using Content.Shared._RMC14.Map;
using Content.Shared._RMC14.Marines;
using Content.Shared._RMC14.Marines.HyperSleep;
using Content.Shared._RMC14.Marines.Squads;
using Content.Shared.Coordinates;
using Content.Shared.Nutrition.Components;
using Content.Shared.Nutrition.EntitySystems;
using Content.Shared.Roles;
using Robust.Server.Containers;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Utility;

namespace Content.Server._MC.Spawn;

public sealed class MCPlayerSpawningSystem : EntitySystem
{
    private const bool SetHunger = false;

    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly ContainerSystem _containers = default!;
    [Dependency] private readonly RMCMapSystem _rmcMap = default!;
    [Dependency] private readonly StationSpawningSystem _stationSpawning = default!;
    [Dependency] private readonly MCSquadSystem _mcSquad = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly SquadSystem _squad = default!;
    [Dependency] private readonly HungerSystem _hunger = default!;

    private EntityQuery<HyperSleepChamberComponent> _hyperSleepChamberQuery;

    public override void Initialize()
    {
        base.Initialize();

        _hyperSleepChamberQuery = GetEntityQuery<HyperSleepChamberComponent>();

        SubscribeLocalEvent<PlayerSpawningEvent>(OnPlayerSpawning, before: [typeof(ArrivalsSystem), typeof(SpawnPointSystem)]);
    }

    private void OnPlayerSpawning(PlayerSpawningEvent ev)
    {
        if (ev.Job is not { } jobId || !_prototype.TryIndex(jobId, out var job) || !job.IsCM)
            return;

        var squadPreference = ev.HumanoidCharacterProfile?.SquadPreference;
        if (GetSpawner(job, squadPreference) is not { } spawnerInfo)
            return;

        var (spawner, squad) = spawnerInfo;
        if (_hyperSleepChamberQuery.TryComp(spawner, out var hyperSleep) && _containers.TryGetContainer(spawner, hyperSleep.ContainerId, out var container))
        {
            ev.SpawnResult = _stationSpawning.SpawnPlayerMob(spawner.ToCoordinates(), ev.Job, ev.HumanoidCharacterProfile, ev.Station);
            _containers.Insert(ev.SpawnResult.Value, container);
        }
        else
        {
            var coordinates = _transform.GetMoverCoordinates(spawner);
            ev.SpawnResult = _stationSpawning.SpawnPlayerMob(coordinates, ev.Job, ev.HumanoidCharacterProfile, ev.Station);
        }

        if (squad is null)
            return;

        _squad.AssignSquad(ev.SpawnResult.Value, squad.Value, jobId);

        if (TryComp(spawner, out TransformComponent? xform) && xform.GridUid is not null)
            EnsureComp<AlmayerComponent>(xform.GridUid.Value);

        if (SetHunger && TryComp<HungerComponent>(ev.SpawnResult, out var hunger))
            _hunger.SetHunger(ev.SpawnResult.Value, 50.0f, hunger);
    }

    private (EntityUid Spawner, EntityUid? Squad)? GetSpawner(JobPrototype job, EntProtoId<SquadTeamComponent>? preferred)
    {
        var allSpawners = GetSpawners();
        EntityUid? squad = null;

        if (job.HasSquad)
        {
            var (squadId, squadEnt) = _mcSquad.NextSquad(job, preferred);
            squad = squadEnt;

            if (allSpawners.Squad.TryGetValue(squadId, out var jobSpawners) && jobSpawners.TryGetValue(job.ID, out var spawners))
                return (_random.Pick(spawners), squadEnt);

            if (allSpawners.SquadAny.TryGetValue(squadId, out var anySpawners))
                return (_random.Pick(anySpawners), squadEnt);

            if (allSpawners.SquadFull.TryGetValue(squadId, out jobSpawners) && jobSpawners.TryGetValue(job.ID, out spawners))
                return (_random.Pick(spawners), squadEnt);

            if (allSpawners.SquadAnyFull.TryGetValue(squadId, out anySpawners))
                return (_random.Pick(anySpawners), squadEnt);

            Log.Error($"No valid spawn found for player. Squad: {squadId}, job: {job.ID}");

            if (allSpawners.NonSquad.TryGetValue(job.ID, out spawners))
                return (_random.Pick(spawners), squadEnt);

            if (allSpawners.NonSquadFull.TryGetValue(job.ID, out spawners))
                return (_random.Pick(spawners), squadEnt);

            Log.Error($"No valid spawn found for player. Job: {job.ID}");
        }
        else
        {
            if (allSpawners.NonSquad.TryGetValue(job.ID, out var spawners))
                return (_random.Pick(spawners), null);

            if (allSpawners.NonSquadFull.TryGetValue(job.ID, out spawners))
                return (_random.Pick(spawners), null);

            Log.Error($"No valid spawn found for player. Job: {job.ID}");
        }

        var pointsQuery = EntityQueryEnumerator<SpawnPointComponent>();
        var jobPoints = new List<EntityUid>();
        var anyJobPoints = new List<EntityUid>();
        var latePoints = new List<EntityUid>();

        while (pointsQuery.MoveNext(out var uid, out var point))
        {
            switch (point.SpawnType)
            {
                case SpawnPointType.Job when point.Job?.Id == job.ID:
                    jobPoints.Add(uid);
                    break;

                case SpawnPointType.Job:
                    anyJobPoints.Add(uid);
                    break;

                case SpawnPointType.LateJoin:
                    latePoints.Add(uid);
                    break;
            }
        }

        if (jobPoints.Count > 0)
            return (_random.Pick(jobPoints), squad);

        if (anyJobPoints.Count > 0)
            return (_random.Pick(anyJobPoints), squad);

        if (latePoints.Count > 0)
            return (_random.Pick(latePoints), squad);

        return null;
    }

    private Spawners GetSpawners()
    {
        var spawners = new Spawners();

        var query = EntityQueryEnumerator<SquadSpawnerComponent>();
        while (query.MoveNext(out var uid, out var spawner))
        {
            if (_hyperSleepChamberQuery.TryComp(uid, out var hyperSleep) && IsHyperSleepFull((uid, hyperSleep)))
            {
                if (spawner.Role is null)
                    spawners.SquadAnyFull.GetOrNew(spawner.Squad).Add(uid);
                else
                    spawners.SquadFull.GetOrNew(spawner.Squad).GetOrNew(spawner.Role.Value).Add(uid);
            }
            else
            {
                var found = false;
                foreach (var cardinal in _rmcMap.CardinalDirections)
                {
                    var anchored = _rmcMap.GetAnchoredEntitiesEnumerator(uid, cardinal);
                    while (anchored.MoveNext(out var anchoredId))
                    {
                        if (!_hyperSleepChamberQuery.TryComp(anchoredId, out hyperSleep))
                            continue;

                        found = true;
                        if (IsHyperSleepFull((anchoredId, hyperSleep)))
                        {
                            if (spawner.Role is null)
                                spawners.SquadAnyFull.GetOrNew(spawner.Squad).Add(anchoredId);
                            else
                                spawners.SquadFull.GetOrNew(spawner.Squad).GetOrNew(spawner.Role.Value).Add(anchoredId);
                        }
                        else
                        {
                            if (spawner.Role is null)
                                spawners.SquadAny.GetOrNew(spawner.Squad).Add(anchoredId);
                            else
                                spawners.Squad.GetOrNew(spawner.Squad).GetOrNew(spawner.Role.Value).Add(anchoredId);
                        }

                        break;
                    }

                    if (found)
                        break;
                }

                if (found)
                    continue;

                if (spawner.Role is null)
                    spawners.SquadAny.GetOrNew(spawner.Squad).Add(uid);
                else
                    spawners.Squad.GetOrNew(spawner.Squad).GetOrNew(spawner.Role.Value).Add(uid);
            }

            continue;

            bool IsHyperSleepFull(Entity<HyperSleepChamberComponent> chamber)
            {
                return _containers.TryGetContainer(chamber, chamber.Comp.ContainerId, out var container) && container.Count > 0;
            }
        }

        var nonSquadQuery = EntityQueryEnumerator<SpawnPointComponent>();
        while (nonSquadQuery.MoveNext(out var uid, out var spawner))
        {
            if (spawner.Job == null)
                continue;

            if (TryComp(uid, out HyperSleepChamberComponent? hyperSleep) &&
                _containers.TryGetContainer(uid, hyperSleep.ContainerId, out var container) &&
                container.Count > 0)
            {
                spawners.NonSquadFull.GetOrNew(spawner.Job.Value).Add(uid);
            }
            else
            {
                spawners.NonSquad.GetOrNew(spawner.Job.Value).Add(uid);
            }
        }

        return spawners;
    }
}

public sealed class Spawners
{
    public readonly Dictionary<EntProtoId, Dictionary<ProtoId<JobPrototype>, List<EntityUid>>> Squad = new();
    public readonly Dictionary<EntProtoId, List<EntityUid>> SquadAny = new();
    public readonly Dictionary<EntProtoId, Dictionary<ProtoId<JobPrototype>, List<EntityUid>>> SquadFull = new();
    public readonly Dictionary<EntProtoId, List<EntityUid>> SquadAnyFull = new();
    public readonly Dictionary<ProtoId<JobPrototype>, List<EntityUid>> NonSquad = new();
    public readonly Dictionary<ProtoId<JobPrototype>, List<EntityUid>> NonSquadFull = new();
}
