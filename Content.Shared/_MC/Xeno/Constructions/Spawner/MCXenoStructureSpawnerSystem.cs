using Content.Shared._MC.Xeno.Hive.Systems.Main;
using Content.Shared._RMC14.Xenonids.Hive;
using Content.Shared.Coordinates;
using Content.Shared.Mobs;
using Robust.Shared.Network;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Shared._MC.Xeno.Constructions.Spawner;

public sealed class MCXenoStructureSpawnerSystem : EntitySystem
{
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly MCStatusSystem _mcStatus = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    [Dependency] private readonly MCSharedXenoHiveSystem _mcXenoHive = null!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MCXenoStructureSpawnerTargetComponent, MobStateChangedEvent>(RemoveFromSpawner);
        SubscribeLocalEvent<MCXenoStructureSpawnerTargetComponent, ComponentShutdown>(RemoveFromSpawner);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<MCXenoStructureSpawnerComponent, HiveMemberComponent>();
        while (query.MoveNext(out var uid, out var component, out var hiveMember))
        {
            if (component.NextSpawn > _timing.CurTime)
                continue;

            if (hiveMember.Hive is null)
                continue;

            var increment = GetIncrement(component);
            component.NextSpawn = _timing.CurTime + increment;
            Dirty(uid, component);

            var mobs = GetMobs(component);
            if (component.Entities.Count > mobs)
                continue;

            if (_net.IsClient)
                continue;

            Spawn((uid, component));
        }
    }

    private void RemoveFromSpawner<TEvent>(Entity<MCXenoStructureSpawnerTargetComponent> entity, ref TEvent args) where TEvent : notnull
    {
        if (!TryComp<MCXenoStructureSpawnerComponent>(entity.Comp.Origin, out var spawnerComponent))
            return;

        spawnerComponent.Entities.Remove(entity);
        Dirty(entity.Comp.Origin, spawnerComponent);
    }

    private TimeSpan GetIncrement(MCXenoStructureSpawnerComponent component)
    {
        var newIncrement = TimeSpan.FromMinutes(3) - TimeSpan.FromSeconds(_mcStatus.ActivePlayerCount * component.RespawnPerPlayer.TotalSeconds);
        return component.RespawnMultiplier * (newIncrement > component.MinRespawn ? newIncrement : component.MinRespawn);
    }

    private int GetMobs(MCXenoStructureSpawnerComponent component)
    {
        return (int) Math.Max(component.MinMobs, component.MobsPerPlayer * _mcStatus.ActivePlayerCount);
    }

    private void Spawn(Entity<MCXenoStructureSpawnerComponent> entity)
    {
        var instance = Spawn(_random.Pick(entity.Comp.Entry), _transform.GetMapCoordinates(entity));

        _mcXenoHive.SetSameHive(entity.Owner, instance);

        var taget = EnsureComp<MCXenoStructureSpawnerTargetComponent>(instance);

        taget.Origin = entity.Owner;
        entity.Comp.Entities.Add(instance);

        Dirty(instance, taget);
        Dirty(entity);
    }
}
