using Robust.Shared.Serialization;

namespace Content.Shared._MC.Engineering.Sentries.UI;

[Serializable, NetSerializable]
public sealed class MCSentryBuiState : BoundUserInterfaceState
{
    public float Integrity = 1f;

    public int Rounds = 300;
    public int MaxRounds = 300;

    public bool RadialMode;
    public bool AlertMode;
}
