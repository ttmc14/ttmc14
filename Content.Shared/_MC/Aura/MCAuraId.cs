using Robust.Shared.Serialization;

namespace Content.Shared._MC.Aura;

[DataDefinition, Serializable, NetSerializable]
public partial struct MCAuraId : IEquatable<MCAuraId>
{
    [DataField]
    public string Value;

    public MCAuraId(string value)
    {
        Value = value;
    }

    public bool Equals(MCAuraId other)
    {
        return Value == other.Value;
    }

    public override bool Equals(object? obj)
    {
        return obj is MCAuraId other && Equals(other);
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }

    public static implicit operator string(MCAuraId id)
    {
        return id.Value;
    }

    public static implicit operator MCAuraId(string value)
    {
        return new MCAuraId(value);
    }
}
