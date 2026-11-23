using Content.Shared._MC.Areas;
using Content.Shared._MC.Engineering.Beacon.Components;
using Content.Shared._MC.Engineering.Beacon.Events;
using Content.Shared._MC.Engineering.Beacon.Prototypes;
using Content.Shared._MC.Engineering.Deploy;
using Content.Shared._MC.Engineering.Deploy.Components;
using Content.Shared._MC.Engineering.Deploy.Events;
using Robust.Shared.Map.Components;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._MC.Engineering.Beacon;

public sealed class MCBeaconSystem : EntitySystem
{
    [Dependency] private readonly MCAreasSystem _areas = null!;
    [Dependency] private readonly SharedMapSystem _map = null!;
    [Dependency] private readonly INetManager _net = null!;

    [Dependency] private readonly MCDeploySystem _mcDeploy = null!;

    private EntityQuery<MCBeaconComponent> _beaconQuery;

    public override void Initialize()
    {
        base.Initialize();

        _beaconQuery = GetEntityQuery<MCBeaconComponent>();

        SubscribeLocalEvent<MCBeaconComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<MCBeaconComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<MCBeaconComponent, MCDeployChangedStateEvent>(OnDeployChangedState);
    }

    private void OnStartup(Entity<MCBeaconComponent> entity, ref ComponentStartup args)
    {
        if (_net.IsClient || !_mcDeploy.Deployed(entity))
            return;

        var ev = new MCBeaconActiveChangedEvent(entity, true);
        RaiseLocalEvent(ref ev);
    }

    private void OnShutdown(Entity<MCBeaconComponent> entity, ref ComponentShutdown args)
    {
        if (!_net.IsServer || !_mcDeploy.Deployed(entity))
            return;

        var ev = new MCBeaconActiveChangedEvent(entity, false);
        RaiseLocalEvent(ref ev);
    }

    public bool Active(EntityUid uid, ProtoId<MCBeaconCategoryPrototype> categoryId)
    {
        if (!_beaconQuery.TryComp(uid, out var component))
            return false;

        return _mcDeploy.Deployed(uid) && component.Category == categoryId;
    }

    public bool Active(NetEntity uid, ProtoId<MCBeaconCategoryPrototype> categoryId)
    {
        return Active(GetEntity(uid), categoryId);
    }

    public List<Entity<MCBeaconComponent>> GetActiveBeacons(ProtoId<MCBeaconCategoryPrototype> categoryId)
    {
        var result =  new List<Entity<MCBeaconComponent>>();
        foreach (var entity in EnumerateActiveBeacons(categoryId))
        {
            result.Add(entity);
        }
        return result;
    }

    public List<NetEntity> GetNetActiveBeacons(ProtoId<MCBeaconCategoryPrototype> categoryId)
    {
        var result = new List<NetEntity>();
        foreach (var entity in EnumerateActiveBeacons(categoryId))
        {
            result.Add(GetNetEntity(entity));
        }
        return result;
    }

    public List<NetBeaconWithName> GetNetActiveBeaconsWithName(ProtoId<MCBeaconCategoryPrototype> categoryId,
        Predicate<EntityUid>? include = null)
    {
        var result = new List<NetBeaconWithName>();
        foreach (var entity in EnumerateActiveBeacons(categoryId))
        {
            if (include != null && !include(entity))
                continue;

            var transform = Transform(entity);
            var name = string.IsNullOrEmpty(entity.Comp.Name) ? Name(entity) : entity.Comp.Name;
            var grid = transform.GridUid;
            var tile = grid is { } uid && TryComp<MapGridComponent>(uid, out var mapGrid)
                ? _map.TileIndicesFor(uid, mapGrid, transform.Coordinates)
                : default;
            result.Add(new NetBeaconWithName(GetNetEntity(entity), name, GetNetEntity(grid), tile));
        }
        return result;
    }

    private void OnDeployChangedState(Entity<MCBeaconComponent> entity, ref MCDeployChangedStateEvent args)
    {
        var deployed = args.State == MCDeployState.Deployed;
        if (deployed && _net.IsServer)
        {
            var area = _areas.GetAreaName(entity);
            entity.Comp.Name = args.User is { } user
                ? Loc.GetString("mc-asrs-beacon-name", ("user", Name(user)), ("area", area))
                : area;
            DirtyField(entity, entity.Comp, nameof(MCBeaconComponent.Name));
        }

        var ev = new MCBeaconActiveChangedEvent(entity, deployed);
        RaiseLocalEvent(ref ev);
    }

    private IEnumerable<Entity<MCBeaconComponent>> EnumerateActiveBeacons(ProtoId<MCBeaconCategoryPrototype> categoryId)
    {
        var query = EntityQueryEnumerator<MCBeaconComponent>();
        while (query.MoveNext(out var uid, out var component))
        {
            if (component.LifeStage >= ComponentLifeStage.Stopping || !_mcDeploy.Deployed(uid))
                continue;

            if (component.Category != categoryId)
                continue;

            yield return (uid, component);
        }
    }

    [Serializable, NetSerializable]
    public readonly record struct NetBeaconWithName(NetEntity Uid, string Name, NetEntity? Grid, Vector2i Tile);
}
