using Content.Shared._MC.ASRS.Components;
using Content.Shared._MC.ASRS.Ui;
using Content.Shared._MC.ASRS.UI.Messages;
using Content.Shared._MC.ASRS.UI.Messages.Approve;
using Content.Shared._MC.ASRS.UI.Messages.Delivery;
using Content.Shared._MC.ASRS.UI.Messages.Deny;

namespace Content.Shared._MC.ASRS.Systems;

public sealed partial class MCASRSConsoleSystem
{
    private static readonly Enum UIKey = MCASRSConsoleUI.Key;

    [Dependency] private readonly SharedUserInterfaceSystem _userInterface = null!;

    private void InitializeUI()
    {
        SubscribeLocalEvent<MCASRSConsoleComponent, BoundUIOpenedEvent>(OnUIOpened);
        SubscribeLocalEvent<MCASRSConsoleComponent, MCASRSConsoleStoreRequestsMessage>(OnRequestMessage);
        SubscribeLocalEvent<MCASRSConsoleComponent, MCASRSConsoleDeliveryMessage>(OnDeliveryMessage);
        SubscribeLocalEvent<MCASRSConsoleComponent, MCASRSConsoleApproveMessage>(OnApproveMessage);
        SubscribeLocalEvent<MCASRSConsoleComponent, MCASRSConsoleApproveAllMessage>(OnApproveAllMessage);
        SubscribeLocalEvent<MCASRSConsoleComponent, MCASRSConsoleDenyMessage>(OnDenyMessage);
        SubscribeLocalEvent<MCASRSConsoleComponent, MCASRSConsoleDenyAllMessage>(OnDenyAllMessage);

        OnRequestUpdated += RefreshUI;
    }

    private void OnUIOpened(Entity<MCASRSConsoleComponent> entity, ref BoundUIOpenedEvent args)
    {
        RefreshUI(entity);
    }

    private void OnRequestMessage(Entity<MCASRSConsoleComponent> entity, ref MCASRSConsoleStoreRequestsMessage args)
    {
        if (entity.Comp.Requests.Count >= entity.Comp.RequestsLimit)
            return;

        var request = MCASRSRequest.Create(GetRequesterName(args.Actor), args.Reason, args.Quantities,
            entity.Comp.CachedEntries, _mcAsrsDrop.CrateCapacity);
        if (request == null)
            return;

        entity.Comp.Requests.Add(request);
        Refresh(entity);
    }

    private void OnDeliveryMessage(Entity<MCASRSConsoleComponent> entity, ref MCASRSConsoleDeliveryMessage args)
    {
        TryDelivery(entity, args.RequestId, args.BeaconUid, args.Actor);
    }

    private void OnApproveAllMessage(Entity<MCASRSConsoleComponent> entity, ref MCASRSConsoleApproveAllMessage args)
    {
        TryApproveAll(entity);
    }

    private void OnApproveMessage(Entity<MCASRSConsoleComponent> entity, ref MCASRSConsoleApproveMessage args)
    {
        var id = args.RequestId;
        if (entity.Comp.Requests.Find(candidate => candidate.Id == id) is { } request)
            TryApprove(entity, request);
    }

    private void OnDenyMessage(Entity<MCASRSConsoleComponent> entity, ref MCASRSConsoleDenyMessage args)
    {
        var id = args.RequestId;
        if (entity.Comp.Requests.Find(candidate => candidate.Id == id) is { } request)
            Deny(entity, request);
    }

    private void OnDenyAllMessage(Entity<MCASRSConsoleComponent> entity, ref MCASRSConsoleDenyAllMessage args)
    {
        Deny(entity, entity.Comp.Requests);
    }

    private void RefreshUI(Entity<MCASRSConsoleComponent> entity)
    {
        if (_net.IsServer)
            _userInterface.SetUiState(entity.Owner, UIKey, GetUIState(entity));
    }

    private MCASRSConsoleBuiState GetUIState(Entity<MCASRSConsoleComponent> entity)
    {
        return new MCASRSConsoleBuiState(
            _mcAsrs.GetBalance(entity.Comp.Account),
            GetDeliveryFee(entity.Comp),
            _mcBeacon.GetNetActiveBeaconsWithName(entity.Comp.DeliveryCategory, IsDeliveryLocation),
            entity.Comp.Requests,
            entity.Comp.RequestsAwaitingDelivery,
            entity.Comp.RequestsDenyHistory,
            entity.Comp.RequestsApprovedHistory);
    }
}
