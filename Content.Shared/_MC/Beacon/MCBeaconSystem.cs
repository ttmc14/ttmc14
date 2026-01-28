using System.Linq;
using Content.Shared._MC.Beacon.Components;
using Content.Shared._MC.Beacon.Events;
using Content.Shared._MC.Beacon.Prototypes;
using Content.Shared._MC.Deploy;
using Content.Shared._MC.Deploy.Events;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Serialization;

namespace Content.Shared._MC.Beacon;

public sealed class MCBeaconSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = null!;
    [Dependency] private readonly INetManager _net = null!;

    [Dependency] private readonly MCDeploySystem _mcDeploy = null!;

    private EntityQuery<MCBeaconComponent> _beaconQuery;

    public override void Initialize()
    {
        base.Initialize();

        _beaconQuery = GetEntityQuery<MCBeaconComponent>();

        SubscribeLocalEvent<MCBeaconComponent, ComponentStartup>(OnMapInit);
        SubscribeLocalEvent<MCBeaconComponent, MCDeployChangedStateEvent>(OnDeployChangedState);
    }

    private void OnMapInit(Entity<MCBeaconComponent> entity, ref ComponentStartup args)
    {
        if (_net.IsClient)
            return;

        if (!string.IsNullOrEmpty(entity.Comp.Name))
            return;

        var symbols = new [] { "A", "B", "G", "D", "X", "Z" };
        entity.Comp.Name = $"{string.Join("", _random.GetItems(symbols, 2))}-{_random.Next(0, 999):000}";

        Dirty(entity);

        if (!_mcDeploy.Deployed(entity))
            return;

        var ev = new MCBeaconActiveChangedEvent(entity, true);
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

    public List<NetBeaconWithName> GetNetActiveBeaconsWithName(ProtoId<MCBeaconCategoryPrototype> categoryId)
    {
        var result = new List<NetBeaconWithName>();
        foreach (var entity in EnumerateActiveBeacons(categoryId))
        {
            result.Add(new NetBeaconWithName(GetNetEntity(entity), entity.Comp.Name));
        }
        return result;
    }

    private void OnDeployChangedState(Entity<MCBeaconComponent> entity, ref MCDeployChangedStateEvent args)
    {
        var ev = new MCBeaconActiveChangedEvent(entity, args.State == MCDeployState.Deployed);
        RaiseLocalEvent(ref ev);
    }

    private IEnumerable<Entity<MCBeaconComponent>> EnumerateActiveBeacons(ProtoId<MCBeaconCategoryPrototype> categoryId)
    {
        var query = EntityQueryEnumerator<MCBeaconComponent>();
        while (query.MoveNext(out var uid, out var component))
        {
            if (!_mcDeploy.Deployed(uid))
                continue;

            if (component.Category != categoryId)
                continue;

            yield return (uid, component);
        }
    }

    [Serializable, NetSerializable]
    public record struct NetBeaconWithName(NetEntity Uid, string Name);
}
