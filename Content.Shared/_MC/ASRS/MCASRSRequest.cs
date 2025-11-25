using Content.Shared._MC.ASRS.Components;
using Robust.Shared.Serialization;

namespace Content.Shared._MC.ASRS;

[DataDefinition, Serializable, NetSerializable]
public sealed partial class MCASRSRequest
{
    [ViewVariables]
    public readonly string Requester;

    [ViewVariables]
    public readonly string Reason;

    [ViewVariables]
    public readonly Dictionary<MCASRSEntry, int> Contents;

    [ViewVariables]
    public readonly int TotalCost;

    public MCASRSRequest(string requester, string reason, Dictionary<MCASRSEntry, int> contents, int totalCost)
    {
        Requester = requester;
        Reason = reason;
        Contents = contents;
        TotalCost = totalCost;
    }

    public override int GetHashCode()
    {
        var hash = new HashCode();

        hash.Add(Requester);
        hash.Add(Reason);

        foreach (var kv in Contents)
        {
            hash.Add(kv.Key);
            hash.Add(kv.Value);
        }

        hash.Add(TotalCost);

        return hash.ToHashCode();
    }

    private bool Equals(MCASRSRequest other)
    {
        return GetHashCode() == other.GetHashCode();
    }

    public override bool Equals(object? obj)
    {
        return ReferenceEquals(this, obj) || obj is MCASRSRequest other && Equals(other);
    }
}
