using Content.Shared._CE.ZLevels.Core.Components;
using Content.Shared._MC;
using Content.Shared.ActionBlocker;
using Content.Shared.Popups;
using JetBrains.Annotations;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Configuration;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Network;
using Robust.Shared.Physics.Components;
using Robust.Shared.Timing;

namespace Content.Shared._CE.ZLevels.Core.EntitySystems;

public abstract partial class CESharedZLevelsSystem : EntitySystem
{
    [Dependency] private readonly INetManager _net = null!;
    [Dependency] private readonly IGameTiming _timing = null!;
    [Dependency] private readonly IConfigurationManager _config = null!;

    [Dependency] private readonly SharedTransformSystem _transform = null!;
    [Dependency] private readonly SharedAudioSystem _audio = null!;
    [Dependency] private readonly ActionBlockerSystem _blocker = null!;
    [Dependency] private readonly EntityLookupSystem _lookup = null!;
    [Dependency] private readonly SharedMapSystem _map = null!;
    [Dependency] private readonly SharedPopupSystem _popup = null!;

    #region Queries

    protected EntityQuery<CEZPhysicsComponent> ZPhysicsQuery;

    private EntityQuery<MapComponent> _mapQuery;
    private EntityQuery<MapGridComponent> _gridQuery;
    private EntityQuery<PhysicsComponent> _physicsQuery;
    private EntityQuery<TransformComponent> _transformQuery;

    private EntityQuery<CEZLevelMapComponent> _zMapQuery;
    private EntityQuery<CEZLevelsNetworkComponent> _zNetworkQuery;
    private EntityQuery<CEZLevelHighGroundComponent> _zHighGroundQuery;

    #endregion

    private bool _clientSimulation;
    private TimeSpan _fixedTimestep;

    public override void Initialize()
    {
        base.Initialize();

        _config.OnValueChanged(MCConfigVars.ZLevelsPhysicsClientSimulation, i => _clientSimulation = i, true);
        _config.OnValueChanged(MCConfigVars.ZLevelsPhysicsTickRate, i => _fixedTimestep = TimeSpan.FromSeconds(1d / i), true);

        #region Queries

        ZPhysicsQuery = GetEntityQuery<CEZPhysicsComponent>();

        _mapQuery = GetEntityQuery<MapComponent>();
        _gridQuery = GetEntityQuery<MapGridComponent>();
        _physicsQuery = GetEntityQuery<PhysicsComponent>();
        _transformQuery = GetEntityQuery<TransformComponent>();

        _zMapQuery = GetEntityQuery<CEZLevelMapComponent>();
        _zNetworkQuery = GetEntityQuery<CEZLevelsNetworkComponent>();
        _zHighGroundQuery = GetEntityQuery<CEZLevelHighGroundComponent>();

        #endregion

        InitializeActivation();
        InitializeCacheHooks();
        InitializeMovement();
        InitializeView();
    }

    public bool IsVoidAtCoordinates(EntityCoordinates coords, out Entity<CEZLevelMapComponent> belowMap)
    {
        belowMap = default;

        var mapId = _transform.GetMapId(coords);
        if (mapId == MapId.Nullspace)
            return false;

        var mapUid = _map.GetMap(mapId);

        if (!_zMapQuery.TryComp(mapUid, out var zMap))
            return false;

        if (!TryMapDown((mapUid, zMap), out belowMap))
            return false;

        // No grid means empty space.
        if (!_gridQuery.TryComp(mapUid, out var grid))
            return true;

        var indices = _map.LocalToTile(mapUid, grid, coords);

        // Avoid unnecessary temp variables.
        return _map
            .GetTileRef(mapUid, grid, indices)
            .Tile
            .IsEmpty;
    }

    /// <summary>
    /// Checks whether the map is in the zLevels network. If so, returns true and the current depth + Entity of the current zLevels network.
    /// </summary>
    [PublicAPI]
    public bool TryGetZNetwork(EntityUid mapUid, out Entity<CEZLevelsNetworkComponent> zLevel)
    {
        zLevel = default;
        if (!_zMapQuery.TryComp(mapUid, out var zMap))
            return false;

        var networkUid = zMap.NetworkUid;
        if (TerminatingOrDeleted(networkUid))
        {
            Log.Error($"Trying access to terminated z-network, map: {mapUid}, outdated network uid: {networkUid}");
            return false;
        }

        if (!_zNetworkQuery.TryComp(networkUid, out var zNetworkComponent))
        {
            Log.Error($"Trying access to z-network without component??? WHY?! map: {mapUid}, network uid: {networkUid}");
            return false;
        }

        zLevel = (networkUid, zNetworkComponent);
        return true;
    }

    [PublicAPI]
    public bool TryMapOffset(Entity<CEZLevelMapComponent?> entity, int offset, out Entity<CEZLevelMapComponent> output)
    {
        output = default;

        if (MapOffset(entity, offset) is not { } result)
            return false;

        output = result;
        return true;
    }

    [PublicAPI]
    public Entity<CEZLevelMapComponent>? MapOffset(Entity<CEZLevelMapComponent?> entity, int offset)
    {
        if (!Resolve(entity, ref entity.Comp, false))
            return null;

        var target = offset switch
        {
             1 => entity.Comp.MapAbove,
            -1 => entity.Comp.MapBelow,
             _ => null,
        };

        if (target is not null && _zMapQuery.TryComp(target.Value, out var component))
            return (target.Value, component);

        if (!_zNetworkQuery.TryComp(entity.Comp.NetworkUid, out var network))
            return null;

        if (!network.ZLevels.TryGetValue(entity.Comp.Depth + offset, out var targetId))
            return null;

        return _zMapQuery.TryComp(targetId, out var comp)
            ? (targetId.Value, comp)
            : null;
    }

    [PublicAPI]
    public bool TryMapUp(Entity<CEZLevelMapComponent?> inputMapUid, out Entity<CEZLevelMapComponent> aboveMapUid) =>
        TryMapOffset(inputMapUid, 1, out aboveMapUid);

    [PublicAPI]
    public bool TryMapDown(Entity<CEZLevelMapComponent?> inputMapUid, out Entity<CEZLevelMapComponent> belowMapUid) =>
        TryMapOffset(inputMapUid, -1, out belowMapUid);

    /// <summary>
    /// Returns a list of all maps above the specified map. The closest map at the top is returned first.
    /// </summary>
    [PublicAPI]
    public List<EntityUid> GetAllMapsAbove(Entity<CEZLevelMapComponent> entity)
    {
        if (!_zNetworkQuery.TryComp(entity.Comp.NetworkUid, out var network) || entity.Comp.Depth >= network.SortedMax)
            return new List<EntityUid>();

        var startIndex = entity.Comp.Depth < network.SortedMin ? 0 : entity.Comp.Depth - network.SortedMin + 1;
        var estimatedCount = network.SortedZLevels.Count - startIndex;
        var result = new List<EntityUid>(estimatedCount);
        var zLevels = network.SortedZLevels;

        for (var i = startIndex; i < zLevels.Count; i++)
        {
            var uid = zLevels[i];
            if (uid == EntityUid.Invalid)
                continue;

            if (_zMapQuery.HasComp(uid))
                result.Add(uid);
        }

        return result;
    }

    [PublicAPI]
    public void GetAllMapsAbove(Entity<CEZLevelMapComponent> entity, List<EntityUid> result)
    {
        result.Clear();

        if (!_zNetworkQuery.TryComp(entity.Comp.NetworkUid, out var network) || entity.Comp.Depth >= network.SortedMax)
            return;

        var startIndex = entity.Comp.Depth < network.SortedMin ? 0 : entity.Comp.Depth - network.SortedMin + 1;
        var zLevels = network.SortedZLevels;

        for (var i = startIndex; i < zLevels.Count; i++)
        {
            var uid = zLevels[i];

            if (uid.IsValid() && _zMapQuery.HasComp(uid))
                result.Add(uid);
        }
    }

    /// <summary>
    /// Returns a list of all maps below the specified map. The closest map at the bottom is returned first.
    /// </summary>
    [PublicAPI]
    public List<EntityUid> GetAllMapsBelow(Entity<CEZLevelMapComponent> entity)
    {
        if (!_zNetworkQuery.TryComp(entity.Comp.NetworkUid, out var network) || entity.Comp.Depth <= network.SortedMin)
            return new List<EntityUid>();

        var endIndex = entity.Comp.Depth - network.SortedMin;
        var result = new List<EntityUid>(endIndex);
        var zLevels = network.SortedZLevels;

        // Iterate backwards for nearest-first ordering.
        for (var i = endIndex - 1; i >= 0; i--)
        {
            var uid = zLevels[i];
            if (uid == EntityUid.Invalid)
                continue;

            if (_zMapQuery.HasComp(uid))
                result.Add(uid);
        }

        return result;
    }
}
