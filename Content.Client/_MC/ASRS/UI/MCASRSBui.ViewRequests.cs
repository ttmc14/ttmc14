using Content.Client._MC.ASRS.UI.Views;
using Content.Client._MC.Beacon.UI;
using Content.Shared._MC.ASRS;
using Content.Shared._MC.ASRS.UI.Messages.Approve;
using Content.Shared._MC.ASRS.UI.Messages.Delivery;
using Content.Shared._MC.ASRS.UI.Messages.Deny;
using Robust.Client.UserInterface;

namespace Content.Client._MC.ASRS.UI;

public sealed partial class MCASRSBui
{
    private MCASRSRequestsView RequestsView => _window.RequestsView;
    private MCASRSRequestsView RequestsAwaitingDeliveryView => _window.RequestsAwaitingDeliveryView;
    private MCASRSRequestsView RequestsApprovedHistoryRequestsView => _window.RequestsApprovedHistoryView;
    private MCASRSRequestsView RequestsDenyHistoryRequestsView => _window.RequestsDeniedHistoryView;

    private void InitializeViewRequests()
    {
        RequestsAwaitingDeliveryView.Reinitialize(false, true);
        RequestsApprovedHistoryRequestsView.Reinitialize(false);
        RequestsDenyHistoryRequestsView.Reinitialize(false);

        RequestsAwaitingDeliveryView.Delivered += OnDelivery;

        RequestsView.ApprovedAll += SendApproveAll;
        RequestsView.DenyAll += SendDenyAll;
        RequestsView.Approved += SendApprove;
        RequestsView.Denied += SendDeny;

        StateRefreshed += RefreshRequests;
    }

    private void RefreshRequests()
    {
        if (_beaconChooseWindow.IsOpen)
            _beaconChooseWindow.Refresh(Beacons, DeliveryFee);

        RefreshRequests(RequestsView, Requests);
        RefreshRequests(RequestsAwaitingDeliveryView, RequestsAwaitingDelivery);
        RefreshRequests(RequestsApprovedHistoryRequestsView, RequestsApprovedHistory);
        RefreshRequests(RequestsDenyHistoryRequestsView, RequestsDeniedHistory);
    }

    private void RefreshRequests(MCASRSRequestsView view, IReadOnlyList<MCASRSRequest> requests)
    {
        view.Refresh(this, requests);
    }

    private void OnDelivery(MCASRSRequest request)
    {
        _beaconChooseWindow.OpenFor(Beacons, request, DeliveryFee);
    }

    private void SendApprove(MCASRSRequest request)
    {
        SendMessage(new MCASRSConsoleApproveMessage(request.Id));
    }

    private void SendDeny(MCASRSRequest request)
    {
        SendMessage(new MCASRSConsoleDenyMessage(request.Id));
    }

    private void SendApproveAll()
    {
        SendMessage(new MCASRSConsoleApproveAllMessage());
    }

    private void SendDenyAll()
    {
        SendMessage(new MCASRSConsoleDenyAllMessage());
    }

    private void OnSelected(NetEntity beaconUid, object? payload)
    {
        if (payload is not MCASRSRequest request)
            return;

        SendMessage(new MCASRSConsoleDeliveryMessage(request.Id, beaconUid));
    }
}
