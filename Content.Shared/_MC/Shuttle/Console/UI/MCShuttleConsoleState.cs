using Robust.Shared.Serialization;

namespace Content.Shared._MC.Shuttle.Console.UI;

[Serializable, NetSerializable]
public sealed class MCShuttleConsoleState(string status, int seconds = 0) : BoundUserInterfaceState
{
    public readonly string Status = status;
    public readonly int Seconds = seconds;
}

[Serializable, NetSerializable]
public enum MCEvacuationPhase
{
    Idle,
    Review,
    Restarting,
    Departing,
    InTransit,
    Complete,
}
