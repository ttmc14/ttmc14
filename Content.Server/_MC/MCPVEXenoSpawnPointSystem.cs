using Content.Shared._MC.Spawners;
using Content.Server.Fluids.EntitySystems;
using Content.Shared.Chemistry.Components;
using Content.Shared.NPC.Components;
using Robust.Shared.Timing;

namespace Content.Server._MC.Spawners;

public sealed class MCPVEXenoSpawnPointSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SmokeSystem _smoke = default!;

    private readonly Dictionary<EntityUid, TimeSpan> _nextSpawnTimes = new();

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<MCPVEXenoSpawnPointComponent, ComponentInit>(OnInit);
        SubscribeLocalEvent<MCPVEXenoSpawnPointComponent, ComponentShutdown>(OnShutdown);
    }

    private void OnInit(EntityUid uid, MCPVEXenoSpawnPointComponent component, ComponentInit args)
    {
        _nextSpawnTimes[uid] = _timing.CurTime + TimeSpan.FromSeconds(5);
    }

    private void OnShutdown(EntityUid uid, MCPVEXenoSpawnPointComponent component, ComponentShutdown args)
    {
        _nextSpawnTimes.Remove(uid);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<MCPVEXenoSpawnPointComponent, TransformComponent>();
        while (query.MoveNext(out var uid, out var component, out var xform))
        {
            if (!_nextSpawnTimes.TryGetValue(uid, out var nextSpawn))
            {
                _nextSpawnTimes[uid] = _timing.CurTime + component.SpawnInterval;
                continue;
            }

            if (_timing.CurTime < nextSpawn)
                continue;

            _nextSpawnTimes[uid] = _timing.CurTime + component.SpawnInterval;

            var coords = xform.Coordinates;

            var spawned = Spawn(component.SpawnPrototype, coords);
            component.SpawnedCount++;

            EnsureComp<NpcFactionMemberComponent>(spawned);
            var factionComp = Comp<NpcFactionMemberComponent>(spawned);
            factionComp.Factions.Add(component.EnemyFaction.Id);

            if (component.SpawnSmokeOnSpawn)
            {
                var solution = new Solution();
                solution.AddReagent(component.SmokeReagent, component.SmokeReagentAmount);

                var smokeEnt = Spawn(component.SmokePrototype, coords);

                _smoke.StartSmoke(smokeEnt, solution, component.SmokeDuration, component.SmokeSpread);
            }

            Dirty(uid, component);
        }
    }
}
