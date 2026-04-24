using Robust.Shared.Serialization;

namespace Content.Shared._MC.Serialization.Loadout.Data;

[DataDefinition, Serializable, NetSerializable]
public sealed partial class MCLoadoutSlot
{
    [DataField(required: true)]
    public string SlotName = string.Empty;

    [DataField(required: true)]
    public MCLoadoutItem? Item;

    public override string ToString()
    {
        return $"Slot: {SlotName}, Item: {Item}";
    }
}
