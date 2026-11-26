using System.Linq;
using Robust.Shared.Serialization;

namespace Content.Shared._MC.Serialization.Loadout.Data;

[DataDefinition, Serializable, NetSerializable]
public sealed partial class MCLoadout
{
    [DataField]
    public string ForkId = string.Empty;

    [DataField]
    public int Version = 3;

    [DataField]
    public List<MCLoadoutSlot> Slots = new();

    public override string ToString()
    {
        var slotsString = string.Join(Environment.NewLine, Slots.Select(s => $"  {s}"));
        return $"ForkId: {ForkId}, Version: {Version}{Environment.NewLine}{slotsString}";
    }
}
