using Content.Shared.FixedPoint;
using Robust.Shared.Serialization;

namespace Content.Shared._MC.Nuke.Generator.UI;

[Serializable, NetSerializable]
public sealed class MCNukeDiskGeneratorOverallProgressBuiState : BoundUserInterfaceState
{
    public readonly FixedPoint2 Value;

    public MCNukeDiskGeneratorOverallProgressBuiState(FixedPoint2 value)
    {
        Value = value;
    }
}
