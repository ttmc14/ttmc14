/*
 * Copyright (c) 2026 TornadgoTechnology
 * Copyright (c) 2026 CrystallEdge (https://github.com/crystallpunk-14/crystall-edge)
 *
 * SPDX-License-Identifier: PolyForm-Noncommercial-1.0.0 AND MIT
 */

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

    private EntityQuery<MapComponent> _mapQuery;
    private EntityQuery<MapGridComponent> _gridQuery;

    private EntityQuery<CEZLevelMapComponent> _zMapQuery;
    private EntityQuery<CEZLevelsNetworkComponent> _zNetworkQuery;

    protected EntityQuery<CEZPhysicsComponent> ZPhyzQuery;

    private bool _clientSimulation;

    public override void Initialize()
    {
        base.Initialize();

        _config.OnValueChanged(MCConfigVars.ZLevelsPhysicsClientSimulation, i => _clientSimulation = i, true);

        _mapQuery = GetEntityQuery<MapComponent>();
        _gridQuery = GetEntityQuery<MapGridComponent>();

        _zMapQuery = GetEntityQuery<CEZLevelMapComponent>();
        _zNetworkQuery = GetEntityQuery<CEZLevelsNetworkComponent>();

        ZPhyzQuery = GetEntityQuery<CEZPhysicsComponent>();

        InitializeActivation();
        InitializeCacheHooks();
        InitializeMovement();
        InitializeView();
    }

    public bool IsVoidAtCoordinates(EntityCoordinates coords, out Entity<CEZLevelMapComponent> belowMap)
    {
        belowMap = default;

        var mapUid = _transform.GetMapId(coords);
        if (mapUid == MapId.Nullspace)
            return false;

        var mapEntity = _map.GetMap(mapUid);
        if (!_zMapQuery.TryComp(mapEntity, out var zMapComp))
            return false;

        if (!TryMapDown((mapEntity, zMapComp), out belowMap))
            return false;

        if (!TryComp<MapGridComponent>(mapEntity, out var mapGridComponent))
            return true;

        var tileIndices = _map.LocalToTile(mapEntity, mapGridComponent, coords);
        var tile = _map.GetTileRef(mapEntity, mapGridComponent, tileIndices);

        return tile.Tile.IsEmpty;
    }

    /// <summary>
    /// Checks whether the map is in the zLevels network. If so, returns true and the current depth + Entity of the current zLevels network.
    /// </summary>
    [PublicAPI]
    public bool TryGetZNetwork(EntityUid mapUid, out Entity<CEZLevelsNetworkComponent> zLevel)
    {
        zLevel = default;
        if (!TryComp<CEZLevelMapComponent>(mapUid, out var zLevelMapComponent))
            return false;

        if (TerminatingOrDeleted(zLevelMapComponent.NetworkUid))
        {
            Log.Error($"Trying access to terminated z-network, map: {mapUid}, outdated network uid: {zLevelMapComponent.NetworkUid}");
            return false;
        }

        if (!TryComp<CEZLevelsNetworkComponent>(zLevelMapComponent.NetworkUid, out var zNetworkComponent))
        {
            Log.Error($"Trying access to z-network without component??? WHY?! map: {mapUid}, network uid: {zLevelMapComponent.NetworkUid}");
            return false;
        }

        zLevel = new Entity<CEZLevelsNetworkComponent>(zLevelMapComponent.NetworkUid, zNetworkComponent);
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

        // Offen we use 1 or -1 for getting maps
        // Because we process this separated for performance boost
        switch (offset)
        {
            case 1 when entity.Comp.MapAbove is not null:
                return new Entity<CEZLevelMapComponent>(entity.Comp.MapAbove.Value, _zMapQuery.GetComponent(entity.Comp.MapAbove.Value));
            case -1 when entity.Comp.MapBelow is not null:
                return new Entity<CEZLevelMapComponent>(entity.Comp.MapBelow.Value, _zMapQuery.GetComponent(entity.Comp.MapBelow.Value));
        }

        if (!_zNetworkQuery.TryComp(entity.Comp.NetworkUid, out var zLevelsNetworkComponent))
            return null;

        var requiredDepth = entity.Comp.Depth + offset;
        if (!zLevelsNetworkComponent.ZLevels.TryGetValue(requiredDepth, out var targetId))
            return null;

        if (!_zMapQuery.TryComp(targetId, out var zLevelMapComponent))
            return null;

        return (targetId.Value, zLevelMapComponent);
    }

    [PublicAPI]
    public bool TryMapUp(Entity<CEZLevelMapComponent?> inputMapUid, out Entity<CEZLevelMapComponent> aboveMapUid)
    {
        return TryMapOffset(inputMapUid, 1, out aboveMapUid);
    }

    [PublicAPI]
    public bool TryMapDown(Entity<CEZLevelMapComponent?> inputMapUid, out Entity<CEZLevelMapComponent> belowMapUid)
    {
        return TryMapOffset(inputMapUid, -1, out belowMapUid);
    }

    /// <summary>
    /// Returns a list of all maps above the specified map. The closest map at the top is returned first.
    /// </summary>
    [PublicAPI]
    public List<EntityUid> GetAllMapsAbove(Entity<CEZLevelMapComponent> mapUid)
    {
        if (!_zNetworkQuery.TryComp(mapUid.Comp.NetworkUid, out var networkComp) || mapUid.Comp.Depth >= networkComp.SortedMax)
            return new List<EntityUid>();

        var startIndex = mapUid.Comp.Depth < networkComp.SortedMin
            ? 0
            : mapUid.Comp.Depth - networkComp.SortedMin + 1;

        var result = new List<EntityUid>();
        for (var i = startIndex; i < networkComp.SortedZLevels.Count; i++)
        {
            var entity = networkComp.SortedZLevels[i];

            if (entity != EntityUid.Invalid && _zMapQuery.HasComp(entity))
                result.Add(entity);
        }

        return result;
    }

    /// <summary>
    /// Returns a list of all maps below the specified map. The closest map at the bottom is returned first.
    /// </summary>
    [PublicAPI]
    public List<EntityUid> GetAllMapsBelow(Entity<CEZLevelMapComponent> mapUid)
    {
        var result = new List<EntityUid>();
        if (!_zNetworkQuery.TryComp(mapUid.Comp.NetworkUid, out var zLevelsNetworkComponent))
            return result;

        var dept = mapUid.Comp.Depth;
        foreach (var mapEntry in zLevelsNetworkComponent.SortedZLevels)
        {
            if (_zMapQuery.TryComp(mapEntry, out var zComp) && zComp.Depth < dept)
                result.Add(mapEntry);
        }

        return result;
    }
}
