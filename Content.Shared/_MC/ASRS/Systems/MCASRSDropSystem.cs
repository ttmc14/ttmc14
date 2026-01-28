using Content.Shared._MC.ASRS.Components;
using Content.Shared._MC.Damage;
using Content.Shared.Storage.EntitySystems;
using Robust.Shared.Map;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Shared._MC.ASRS.Systems;

public sealed class MCASRSDropSystem : EntitySystem
{
    private static readonly TimeSpan DefaultDropDelay = TimeSpan.FromSeconds(5);
    private static readonly EntProtoId DefaultCrateId = "MCCrate";
    private static readonly EntProtoId DefaultLandingEffectId = "RMCEffectAlert";

    [Dependency] private readonly IGameTiming _timing = null!;
    [Dependency] private readonly INetManager _net = null!;

    [Dependency] private readonly SharedEntityStorageSystem _entityStorage = default!;
    [Dependency] private readonly EntityLookupSystem _entityLookup = null!;
    [Dependency] private readonly SharedTransformSystem _transform = null!;
    [Dependency] private readonly SharedMapSystem _map = null!;

    [Dependency] private readonly MCDamageableSystem _mcDamageable = null!;

    private int _tempMapOffset;
    private MapId? _tempMap;

    private MapId TempMap => EnsureMap();

    public override void Update(float frameTime)
    {
        if (_net.IsClient)
            return;

        var query = EntityQueryEnumerator<MCASRSDroppedComponent>();
        while (query.MoveNext(out var uid, out var component))
        {
            if (_timing.CurTime < component.DropTime)
                continue;

            if (!TerminatingOrDeleted(component.EffectUid))
                Del(component.EffectUid);

            foreach (var intersecting in _entityLookup.GetEntitiesInRange(component.TargetCoordinates, 0.33f))
            {
                _mcDamageable.AdjustBruteLoss(intersecting, 1_000_000); // FUCK YOU!!!!
            }

            _transform.SetCoordinates(uid, component.TargetCoordinates);
            RemCompDeferred<MCASRSDroppedComponent>(uid);
        }
    }

    public void Drop(MCASRSRequest request, EntityUid beaconUid)
    {
        Drop(request, _transform.GetMoverCoordinates(beaconUid));
    }

    public void Drop(MCASRSRequest request, EntityCoordinates coordinates)
    {
        if (_net.IsClient)
            return;

        var create = SpawnCrate(request);
        create.Comp.TargetCoordinates = coordinates;
        create.Comp.EffectUid = SpawnAttachedTo(DefaultLandingEffectId, coordinates);
        create.Comp.DropTime = _timing.CurTime + DefaultDropDelay;
        Dirty(create);
    }

    private Entity<MCASRSDroppedComponent> SpawnCrate(MCASRSRequest request)
    {
        var uid = Spawn(DefaultCrateId, new MapCoordinates(_tempMapOffset++, 0, TempMap));
        FillCrate(uid, request);
        return (uid, EnsureComp<MCASRSDroppedComponent>(uid));
    }

    private MapId EnsureMap()
    {
        var map = _tempMap;
        if (map.HasValue && _map.MapExists(map))
            return map.Value;

        _map.CreateMap(out var newMap);
        _tempMap = newMap;

        return newMap;
    }

    private void FillCrate(EntityUid uid, MCASRSRequest request)
    {
        var coordinates = Transform(uid).Coordinates;
        foreach (var (entry, count) in request.Contents)
        {
            for (var i = 0; i < count; i++)
            {
                foreach (var itemId in entry.Entities)
                {
                    var ent = Spawn(itemId, coordinates);
                    if (_entityStorage.Insert(ent, uid))
                        continue;

                    Log.Error($"Tried to StorageFill {itemId} inside {ToPrettyString(uid)} but can't.");
                    Del(ent);
                }
            }
        }
    }
}
