using Content.Shared._MC.Engineering.Beacon;
using Robust.Shared.Serialization;

namespace Content.Shared._MC.ASRS.Ui;

[Serializable, NetSerializable]
public sealed class MCASRSConsoleBuiState(
    int points,
    List<MCBeaconSystem.NetBeaconWithName> beacons,
    List<MCASRSRequest> requests,
    List<MCASRSRequest> requestsAwaitingDelivery,
    List<MCASRSRequest> requestsDeniedHistory,
    List<MCASRSRequest> requestsApprovedHistory)
    : BoundUserInterfaceState
{
    public readonly int Points = points;

    public readonly List<MCBeaconSystem.NetBeaconWithName> Beacons = beacons;

    public readonly List<MCASRSRequest> Requests = requests;
    public readonly List<MCASRSRequest> RequestsAwaitingDelivery = requestsAwaitingDelivery;

    public readonly List<MCASRSRequest> RequestsApprovedHistory = requestsApprovedHistory;
    public readonly List<MCASRSRequest> RequestsDeniedHistory = requestsDeniedHistory;
}
