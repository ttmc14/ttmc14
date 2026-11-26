using System.Linq;
using Content.Shared._MC.ASRS.Components;
using Content.Shared._RMC14.Map;
using Content.Shared._RMC14.Rules;
using Content.Shared.Maps;
using Content.Shared.Physics;
using Content.Shared.Popups;
using Robust.Shared.Network;

namespace Content.Shared._MC.ASRS.Systems;

public sealed partial class MCASRSConsoleSystem : EntitySystem
{
    [Dependency] private readonly MCASRSDropSystem _mcAsrsDrop = null!;
    [Dependency] private readonly RMCPlanetSystem _planet = null!;
    [Dependency] private readonly RMCMapSystem _map = null!;
    [Dependency] private readonly TurfSystem _turf = null!;
    [Dependency] private readonly SharedPopupSystem _popup = null!;
    [Dependency] private readonly INetManager _net = null!;

    private event Action<Entity<MCASRSConsoleComponent>>? OnRequestUpdated;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MCASRSConsoleComponent, ComponentInit>(OnInit);

        InitializeBalance();
        InitializeBeacon();
        InitializeUI();
    }

    private void OnInit(Entity<MCASRSConsoleComponent> entity, ref ComponentInit args)
    {
        Cache(entity);
        Refresh(entity);
    }

    private void TryDelivery(Entity<MCASRSConsoleComponent> entity, Guid requestId, NetEntity beaconUid, EntityUid user)
    {
        if (_net.IsClient)
            return;

        var pending = entity.Comp.RequestsAwaitingDelivery;
        var index = pending.FindIndex(request => request.Id == requestId);
        if (index < 0)
            return;

        if (!TryGetEntity(beaconUid, out var beacon) || !_mcBeacon.Active(beacon.Value, entity.Comp.DeliveryCategory) ||
            !IsDeliveryLocation(beacon.Value))
        {
            _popup.PopupEntity(Loc.GetString("mc-asrs-beacon-unavailable"), entity, user);
            return;
        }

        var coords = Transform(beacon.Value).Coordinates;
        if (!_map.TryGetTileRefForEnt(coords, out _, out var tile) || _turf.IsSpace(tile) ||
            _turf.IsTileBlocked(tile, CollisionGroup.Impassable))
        {
            _popup.PopupEntity(Loc.GetString("mc-asrs-drop-blocked"), entity, user);
            return;
        }

        var fee = entity.Comp.DeliveryFee;
        if (!_mcAsrs.HasBalance(fee, entity.Comp.Account))
        {
            _popup.PopupEntity(Loc.GetString("mc-asrs-delivery-insufficient", ("fee", fee)), entity, user);
            return;
        }

        _mcAsrsDrop.Drop(pending[index], beacon.Value);
        pending.RemoveAt(index);
        if (fee > 0)
            _mcAsrs.RemoveBalance(fee, entity.Comp.Account);
        Refresh(entity);
    }

    private void TryApprove(Entity<MCASRSConsoleComponent> entity, MCASRSRequest request)
    {
        if (!_mcAsrs.TryRemoveBalance(request.TotalCost, entity.Comp.Account))
            return;

        Approve(entity, request);
    }

    private void TryApproveAll(Entity<MCASRSConsoleComponent> entity)
    {
        var requests = entity.Comp.Requests;
        if (requests.Count == 0 || requests.Any(request => request.TotalCost < 0))
            return;

        var totalCost = requests.Sum(request => (long) request.TotalCost);
        if (totalCost > int.MaxValue || !_mcAsrs.TryRemoveBalance((int) totalCost, entity.Comp.Account))
            return;

        Approve(entity, requests);
    }

    private void Approve(Entity<MCASRSConsoleComponent> entity, MCASRSRequest request)
    {
        entity.Comp.Requests.Remove(request);
        entity.Comp.RequestsAwaitingDelivery.Add(request);
        HistoryWrite(entity.Comp.RequestsApprovedHistory, request, entity.Comp.RequestsHistoryLimit);
        Refresh(entity);
    }

    private void Approve(Entity<MCASRSConsoleComponent> entity, List<MCASRSRequest> requests)
    {
        foreach (var request in new List<MCASRSRequest>(requests))
        {
            Approve(entity, request);
        }
    }

    private void Deny(Entity<MCASRSConsoleComponent> entity, MCASRSRequest request)
    {
        entity.Comp.Requests.Remove(request);
        HistoryWrite(entity.Comp.RequestsDenyHistory, request, entity.Comp.RequestsHistoryLimit);
        Refresh(entity);
    }

    private void Deny(Entity<MCASRSConsoleComponent> entity, List<MCASRSRequest> requests)
    {
        foreach (var request in new List<MCASRSRequest>(requests))
        {
            Deny(entity, request);
        }
    }

    private void RefreshAll(bool dirty = true)
    {
        var query = EntityQueryEnumerator<MCASRSConsoleComponent>();
        while (query.MoveNext(out var uid, out var component))
        {
            Refresh((uid, component), dirty);
        }
    }

    private void Refresh(Entity<MCASRSConsoleComponent> entity, bool dirty = true)
    {
        if (dirty)
            Dirty(entity);

        OnRequestUpdated?.Invoke(entity);
    }

    private static void HistoryWrite(List<MCASRSRequest> container, MCASRSRequest element, int limit)
    {
        if (container.Count > limit)
        {
            container.RemoveAt(0);
            return;
        }

        container.Add(element);
    }

    private static void Cache(Entity<MCASRSConsoleComponent> entity)
    {
        entity.Comp.CachedEntries.Clear();
        foreach (var category in entity.Comp.Categories)
        {
            entity.Comp.CachedEntries.AddRange(category.Entries);
        }
    }
}
