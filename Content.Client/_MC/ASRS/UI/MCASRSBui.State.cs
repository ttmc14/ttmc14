using System.Linq;
using Content.Shared._MC.ASRS;
using Content.Shared._MC.ASRS.Ui;
using Content.Shared._MC.Engineering.Beacon;

namespace Content.Client._MC.ASRS.UI;

public sealed partial class MCASRSBui
{
    private event Action? StateRefreshed;

    public List<MCBeaconSystem.NetBeaconWithName> Beacons = new();
    public List<MCASRSRequest> Requests = new();
    public List<MCASRSRequest> RequestsAwaitingDelivery = new();
    public List<MCASRSRequest> RequestsApprovedHistory = new();
    public List<MCASRSRequest> RequestsDeniedHistory = new();

    public int Points { get; private set; }
    public int RequestsTotalCost { get; private set; }

    public bool HasRequests => Requests.Count > 0;
    public bool HasRequestsAwaitingDelivery => RequestsAwaitingDelivery.Count > 0;
    public bool HasRequestsApprovedHistory => RequestsApprovedHistory.Count > 0;
    public bool HasRequestsDeniedHistory => RequestsDeniedHistory.Count > 0;

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);

        if (state is MCASRSConsoleBuiState castedState)
            UpdateState(castedState);
    }

    private void UpdateState(MCASRSConsoleBuiState state)
    {
        Beacons = state.Beacons;

        Requests = state.Requests;
        RequestsAwaitingDelivery = state.RequestsAwaitingDelivery;
        RequestsApprovedHistory = state.RequestsApprovedHistory;
        RequestsDeniedHistory = state.RequestsDeniedHistory;

        Points = state.Points;
        RequestsTotalCost = Requests.Sum(request => request.TotalCost);

        StateRefreshed?.Invoke();
    }
}
