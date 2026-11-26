using Robust.Shared.Serialization;

namespace Content.Shared._MC.Serialization.Loadout.Data;

/// <summary>
/// A snapshot owned by one loadout handler. Concrete types are serialized as YAML type tags.
/// </summary>
[ImplicitDataDefinitionForInheritors, Serializable, NetSerializable]
public abstract partial class MCLoadoutPart
{
    public virtual int RestoreOrder => 0;

    public virtual IEnumerable<MCLoadoutItem> GetContents() => Array.Empty<MCLoadoutItem>();
}

[Serializable, NetSerializable]
public abstract partial class MCLoadoutSlotsPart : MCLoadoutPart
{
    [DataField]
    public Dictionary<string, MCLoadoutItem> Slots = new();

    public override IEnumerable<MCLoadoutItem> GetContents() => Slots.Values;
}
