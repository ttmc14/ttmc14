using Robust.Shared.Serialization;

namespace Content.Shared._MC.ASRS.Ui;

[Serializable, NetSerializable]
public sealed class MCASRSConsoleBuiState : BoundUserInterfaceState
{
    public readonly int Points;
    public readonly List<MCASRSRequest> Requests;

    public MCASRSConsoleBuiState(int points, List<MCASRSRequest> requests)
    {
        Points = points;
        Requests = requests;
    }
}
