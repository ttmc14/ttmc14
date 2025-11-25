using Robust.Shared.Serialization;

namespace Content.Shared._MC.Nuke.Bomb.UI;

[Serializable, NetSerializable]
public sealed class MCNukeBuiState : BoundUserInterfaceState
{
    public readonly bool Ready;
    public readonly bool Safety;
    public readonly bool Activated;
    public readonly bool Anchored;

    public MCNukeBuiState(bool ready, bool safety, bool activated, bool anchored)
    {
        Ready = ready;
        Safety = safety;
        Activated = activated;
        Anchored = anchored;
    }
}
